using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// Provisions a declared scenario through the product's own routes: signing up, approving, inviting and changing
/// status, so the flows a person uses are the flows being exercised.
/// </summary>
/// <remarks>
/// Two secrets are needed along the way and neither is guessable: the password a sign-up generates, and the
/// password an invitation generates. Both are read from the messages the application sent, through the helpers
/// that exist for exactly that purpose. The one-time code a sign-up verifies is read from the store, where it is
/// kept, because it is never the point of the test.
/// </remarks>
internal sealed class ScenarioProvisioner
{
    private sealed class RoleOption
    {
        public Guid RowId { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public string? AccessToken { get; set; }
    }

    private const string ApprovalsPath = "/api/approvals";
    private const string PendingApprovalsPath = $"{ApprovalsPath}/pending";
    private const string UsersPath = "/api/users";
    private const string RolesPath = "/api/roles";
    private const string LoginPath = "/api/auth/login";
    private const string SignUpSendPath = "/api/auth/signup/send";
    private const string SignUpCompletePath = "/api/auth/signup/complete";

    private const string ValidateStep = "validate the declaration";
    private const string SignUpStep = "sign up the owner";
    private const string CompleteSignUpStep = "complete the sign up";
    private const string ReadCodesStep = "read the one-time code";
    private const string ReadUserStep = "read the created user";
    private const string ApproveStep = "approve the owner";
    private const string ListApprovalsStep = "list the pending approvals";
    private const string SignInStep = "sign in";
    private const string InviteStep = "invite a member";
    private const string DisableStep = "disable a member";
    private const string DirectWriteStep = "write the declared state";

    private readonly IScenarioHost _host;

    public ScenarioProvisioner(IScenarioHost host)
    {
        _host = host.WhenNotNull();
    }

    /// <summary>
    /// Provisions the scenario and returns what a test needs to act in it.
    /// </summary>
    /// <param name="definition">The scenario to provision.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The provisioned tenant and its people.</returns>
    /// <exception cref="ScenarioProvisioningException">A step could not be completed.</exception>
    public async Task<ProvisionedScenario> ProvisionAsync(ScenarioDefinition definition, CancellationToken cancellationToken)
    {
        _ = definition.WhenNotNull();

        Validate(definition);

        var suffix = Guid.NewGuid().ToString("N")[..12];

        var owner = await ProvisionOwnerAsync(definition.Name, suffix, cancellationToken);

        using var ownerClient = await SignInAsync(owner, cancellationToken);

        var members = await ProvisionMembersAsync(definition, suffix, ownerClient, cancellationToken);

        // Read while the owner can still act, because a declared direct write may disable them.
        var currentUser = await ownerClient.GetCurrentUserAsync();

        await ApplyDirectWriteAsync(definition, owner, cancellationToken);

        return new ProvisionedScenario(definition.Name, currentUser.Site.RowId, owner, members);
    }

    private async Task<ScenarioUser> ProvisionOwnerAsync(string name, string suffix, CancellationToken cancellationToken)
    {
        var username = $"{name}-owner-{suffix}";
        var email = $"{username}@example.com";

        using (var client = _host.CreateAnonymousClient())
        {
            var request = new
            {
                Username = username,
                Email = email
            };

            using var response = await client.PostAsJsonAsync(SignUpSendPath, request, cancellationToken);

            await EnsureSucceededAsync(SignUpStep, username, response, cancellationToken);
        }

        var (verificationCode, referenceCode) = await ReadSignUpCodesAsync(username, cancellationToken);

        using (var client = _host.CreateAnonymousClient())
        {
            var request = new
            {
                Username = username,
                ReferenceCode = referenceCode,
                VerificationCode = verificationCode
            };

            using var response = await client.PostAsJsonAsync(SignUpCompletePath, request, cancellationToken);

            await EnsureSucceededAsync(CompleteSignUpStep, username, response, cancellationToken);
        }

        var userRowId = await ReadUserRowIdAsync(username, cancellationToken);

        await ApproveOwnerAsync(userRowId, username, cancellationToken);

        var password = _host.Emails.GetSignUpPassword(email);

        return new ScenarioUser(userRowId, username, password);
    }

    private async Task ApproveOwnerAsync(Guid userRowId, string username, CancellationToken cancellationToken)
    {
        using var approver = await _host.CreatePlatformApproverClientAsync(cancellationToken);

        using var pendingResponse = await approver.GetAsync(PendingApprovalsPath, cancellationToken);

        await EnsureSucceededAsync(ListApprovalsStep, username, pendingResponse, cancellationToken);

        var pending = await pendingResponse.Content.ReadFromJsonAsync<PendingApprovalResponse[]>(cancellationToken);
        var approval = pending?.SingleOrDefault(item => item.Username == username);

        if (approval is null)
        {
            throw new ScenarioProvisioningException(ApproveStep,
                $"'{username}' was not among the pending approvals ({pending?.Length ?? 0} listed).");
        }

        var request = new
        {
            Etag = approval.Etag,
            Status = ApprovalStatus.Approved.Name
        };

        using var response = await approver.PutAsJsonAsync($"{ApprovalsPath}/{approval.RowId}/status", request, cancellationToken);

        await EnsureSucceededAsync(ApproveStep, username, response, cancellationToken);

        // The list and the decision both name the user, so a mismatch would approve the wrong person.
        approval.RowId.ShouldBe(userRowId, $"{ApproveStep}: the named user and the approved user must be the same");
    }

    private async Task<IReadOnlyDictionary<string, ScenarioUser>> ProvisionMembersAsync(ScenarioDefinition definition,
        string suffix, HttpClient ownerClient, CancellationToken cancellationToken)
    {
        var members = new Dictionary<string, ScenarioUser>(StringComparer.OrdinalIgnoreCase);

        if (definition.Members.Count == 0)
        {
            return members;
        }

        var roles = await ReadRolesAsync(ownerClient, cancellationToken);

        foreach (var member in definition.Members)
        {
            var username = $"{definition.Name}-{member.Name}-{suffix}";
            var email = $"{username}@example.com";

            await InviteAsync(ownerClient, member, username, email, roles, cancellationToken);

            var password = _host.Emails.GetInvitationPassword(email);
            var userRowId = await ReadUserRowIdAsync(username, cancellationToken);
            var memberUser = new ScenarioUser(userRowId, username, password);

            await ActivateInvitationAsync(memberUser, cancellationToken);

            if (member.Disabled)
            {
                await DisableAsync(ownerClient, username, cancellationToken);
            }

            members.Add(member.Name, memberUser);
        }

        return members;
    }

    private async Task InviteAsync(HttpClient ownerClient, ScenarioMember member, string username, string email,
        IReadOnlyDictionary<string, Guid> roles, CancellationToken cancellationToken)
    {
        if (!roles.TryGetValue(member.Role.Name, out var roleRowId))
        {
            throw new ScenarioProvisioningException(InviteStep,
                $"The role '{member.Role.Name}' is not offered by the endpoint, so '{member.Name}' cannot be invited with it.");
        }

        var request = new
        {
            Username = username,
            Email = email,
            RoleIds = new[] { roleRowId }
        };

        using var response = await ownerClient.PostAsJsonAsync($"{UsersPath}/invite", request, cancellationToken);

        await EnsureSucceededAsync(InviteStep, username, response, cancellationToken);
    }

    private async Task ActivateInvitationAsync(ScenarioUser user, CancellationToken cancellationToken)
    {
        // An invited account becomes enabled on its first sign-in, which is the state the product leaves it in.
        using var client = await SignInAsync(user, cancellationToken);
    }

    private async Task DisableAsync(HttpClient ownerClient, string username, CancellationToken cancellationToken)
    {
        var users = await ownerClient.GetFromJsonAsync<UserResponse[]>(UsersPath, cancellationToken);
        var listed = users?.SingleOrDefault(user => user.Username == username);

        if (listed is null)
        {
            throw new ScenarioProvisioningException(DisableStep, $"'{username}' was not in the user list.");
        }

        var request = new
        {
            Etag = listed.Etag,
            Status = UserStatus.Disabled.Name
        };

        using var response = await ownerClient.PutAsJsonAsync($"{UsersPath}/{listed.RowId}/status", request, cancellationToken);

        await EnsureSucceededAsync(DisableStep, username, response, cancellationToken);
    }

    private async Task ApplyDirectWriteAsync(ScenarioDefinition definition, ScenarioUser owner, CancellationToken cancellationToken)
    {
        if (definition.DirectWrite is null)
        {
            return;
        }

        using var scope = _host.CreateDataScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        // Tracking, because the context reads detached copies by default and a detached copy would be discarded.
        var user = await dbContext.Set<UserEntity>()
            .AsTracking()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(entity => entity.RowId == owner.UserRowId, cancellationToken);

        if (user is null)
        {
            throw new ScenarioProvisioningException(DirectWriteStep, $"'{owner.Username}' could not be found to write its state.");
        }

        user.Status = definition.DirectWrite.Status;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<HttpClient> SignInAsync(ScenarioUser user, CancellationToken cancellationToken)
    {
        var client = _host.CreateAnonymousClient();
        var request = new
        {
            user.Username,
            user.Password
        };

        using var response = await client.PostAsJsonAsync(LoginPath, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            client.Dispose();

            throw new ScenarioProvisioningException(SignInStep,
                $"'{user.Username}' was refused with {(int)response.StatusCode} {response.StatusCode}: {body}");
        }

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);

        if (string.IsNullOrWhiteSpace(loginResult?.AccessToken))
        {
            client.Dispose();

            throw new ScenarioProvisioningException(SignInStep,
                $"'{user.Username}' authenticated but was issued no access token, which means the account is not usable in its current state.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        return client;
    }

    private async Task<(string VerificationCode, string ReferenceCode)> ReadSignUpCodesAsync(string username,
        CancellationToken cancellationToken)
    {
        using var scope = _host.CreateDataScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var otp = await dbContext.Set<OneTimePasswordEntity>()
            .AsNoTracking()
            .Where(entity => entity.Username == username)
            .OrderByDescending(entity => entity.CreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null)
        {
            throw new ScenarioProvisioningException(ReadCodesStep, $"No one-time code was issued for '{username}'.");
        }

        return (otp.OtpCode, otp.RefCode);
    }

    private async Task<Guid> ReadUserRowIdAsync(string username, CancellationToken cancellationToken)
    {
        using var scope = _host.CreateDataScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var user = await dbContext.Set<UserEntity>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(entity => entity.Username == username, cancellationToken);

        if (user is null)
        {
            throw new ScenarioProvisioningException(ReadUserStep, $"No user was created for '{username}'.");
        }

        return user.RowId;
    }

    private static async Task<IReadOnlyDictionary<string, Guid>> ReadRolesAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var roles = await client.GetFromJsonAsync<RoleOption[]>(RolesPath, cancellationToken);

        return roles!.ToDictionary(role => role.Name, role => role.RowId, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task EnsureSucceededAsync(string step, string subject, HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new ScenarioProvisioningException(step,
            $"'{subject}' was refused with {(int)response.StatusCode} {response.StatusCode}: {body}");
    }

    private static void Validate(ScenarioDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Name))
        {
            throw new ScenarioProvisioningException(ValidateStep, "The scenario needs a name.");
        }

        var repeated = definition.Members
            .GroupBy(member => member.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (repeated.Count > 0)
        {
            throw new ScenarioProvisioningException(ValidateStep,
                $"A member's name labels it within the scenario, so it must be unique; repeated: {string.Join(", ", repeated)}.");
        }

        if (definition.DirectWrite is not null && string.IsNullOrWhiteSpace(definition.DirectWrite.Reason))
        {
            throw new ScenarioProvisioningException(ValidateStep,
                "A direct write must state why no route can produce the state, so the exception stays visible.");
        }
    }
}
