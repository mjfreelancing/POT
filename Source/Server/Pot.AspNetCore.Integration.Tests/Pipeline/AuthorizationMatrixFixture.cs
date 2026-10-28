using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Concerns.Auth.Services;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.AspNetCore.Integration.Tests.Pipeline.Support;
using Pot.AspNetCore.Integration.Tests.Scenarios;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// Proves every documented operation answers a caller according to the authorization it declares, including a caller
/// naming a row that belongs to another tenant.
/// </summary>
/// <remarks>
/// The cases are generated from the contract and the expectations from each endpoint's own declaration, so an
/// operation added later is exercised and judged without anyone listing it. A request is only ever sent as a caller
/// the declaration should refuse, or as a caller it should let through to fail on its own terms, which is why a case
/// asserts the authorization decision rather than a business outcome.
/// </remarks>
public partial class AuthorizationMatrixFixture : ScenarioFixtureBase
{
    private const string RowParameter = "{id}";
    private const string UnusedParameterValue = "unknown";

    // A caller the declaration lets through reaches the row lookup, so the case needs a row that exists nowhere.
    private static readonly Guid MissingRowId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex PathParameterPattern();

    private ProvisionedScenario? _target;
    private ForeignTenantRows? _foreignRows;
    private DevelopmentApiWebApplicationFactory? _contractFactory;
    private HttpClient? _administratorClient;
    private HttpClient? _viewerClient;
    private HttpClient? _anonymousClient;

    [Fact]
    public async Task Should_Exercise_Every_Documented_Operation_Or_Ignore_It_With_A_Reason()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await EnsureTenantsAsync(cancellationToken);

        var inventory = await ReadInventoryAsync(cancellationToken);
        var cases = AuthorizationMatrix.Generate(inventory, IgnoredOperations.All);

        var problems = new List<string>();

        problems.AddRange(InventoryCoverage.FindProblems(
            inventory.Select(documented => documented.Operation),
            AuthorizationMatrix.CoveredOperations(cases),
            IgnoredOperations.All));

        problems.AddRange(ForeignRows.FindUnclassifiedOperations(inventory.Select(documented => documented.Operation)));

        using (var scope = CreateScope())
        {
            var requirements = EndpointRequirements.Read(scope.ServiceProvider);

            problems.AddRange(inventory
                .Select(documented => documented.Operation)
                .Where(operation => !requirements.ContainsKey(operation))
                .Select(operation =>
                    $"{operation} is documented but no mapped endpoint answers it, so its declaration cannot be read"));
        }

        problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public async Task Should_Answer_Each_Documented_Operation_As_Its_Own_Declaration_Says()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await EnsureTenantsAsync(cancellationToken);

        var inventory = await ReadInventoryAsync(cancellationToken);
        var cases = AuthorizationMatrix.Generate(inventory, IgnoredOperations.All);

        IReadOnlyDictionary<EndpointOperation, EndpointRequirement> requirements;

        using (var scope = CreateScope())
        {
            requirements = EndpointRequirements.Read(scope.ServiceProvider);
        }

        var permissions = await GetCallerPermissionsAsync(cancellationToken);

        var failures = new List<string>();

        foreach (var authorizationCase in cases)
        {
            if (!requirements.TryGetValue(authorizationCase.Operation, out var requirement))
            {
                failures.Add($"{authorizationCase}: no mapped endpoint answers it, so its declaration cannot be read");
                continue;
            }

            var problem = await CheckAsync(authorizationCase, requirement, permissions[authorizationCase.Caller],
                cancellationToken);

            if (problem is not null)
            {
                failures.Add(problem);
            }
        }

        failures.ShouldBeEmpty(
            $"{failures.Count} of {cases.Count} authorization cases disagreed with the endpoint's own declaration:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    [Fact]
    public async Task Should_Answer_A_Foreign_Tenants_Row_As_Not_Found()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await EnsureTenantsAsync(cancellationToken);

        using var client = await GetCallerClientAsync(AuthorizationCaller.Administrator, cancellationToken);

        string[] paths =
        [
            $"/api/accounts/{ForeignRows.AccountRowId}",
            $"/api/expenses/{ForeignRows.ExpenseRowId}",
            $"/api/incomes/{ForeignRows.IncomeRowId}"
        ];

        foreach (var path in paths)
        {
            using var response = await client.GetAsync(path, cancellationToken);

            // Reading a row the caller's tenant does not own must be indistinguishable from reading one that was
            // deleted, otherwise the path reports which identifiers exist in other tenants.
            response.StatusCode.ShouldBe(
                HttpStatusCode.NotFound,
                $"{path} names another tenant's row and must be answered as though the row did not exist");
        }
    }

    private ForeignTenantRows ForeignRows
    {
        get
        {
            return _foreignRows.ShouldNotBeNull("the tenants are provisioned before any case runs");
        }
    }

    private static string DescribeExpectation(AuthorizationCase authorizationCase, EndpointRequirement requirement,
        IReadOnlySet<string> permissions)
    {
        if (authorizationCase.Caller == AuthorizationCaller.Anonymous && !requirement.AllowsAnonymous)
        {
            return $"expected 401 because the endpoint {requirement.Describe()}";
        }

        if (!requirement.Allows(permissions))
        {
            return $"expected 403 because the endpoint {requirement.Describe()} and this caller does not hold it";
        }

        if (!authorizationCase.RowIsForeign)
        {
            return $"expected the endpoint {requirement.Describe()} to let this caller through";
        }

        return authorizationCase.CarriesRequestBody
            ? "expected the request to be refused without disclosing that the row exists"
            : "expected 404 because the row belongs to another tenant";
    }

    private static bool Satisfies(AuthorizationCase authorizationCase, EndpointRequirement requirement,
        IReadOnlySet<string> permissions, HttpStatusCode status)
    {
        if (authorizationCase.Caller == AuthorizationCaller.Anonymous && !requirement.AllowsAnonymous)
        {
            return status == HttpStatusCode.Unauthorized;
        }

        if (!requirement.Allows(permissions))
        {
            return status == HttpStatusCode.Forbidden;
        }

        if (!authorizationCase.RowIsForeign)
        {
            return status is not (HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
        }

        if (!authorizationCase.CarriesRequestBody)
        {
            return status == HttpStatusCode.NotFound;
        }

        // A body that cannot be validated is refused before the row is read, so a body-carrying operation can only be
        // held to disclosing nothing: never a success, and never a refusal that admits the row exists.
        return status is not (HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) && !IsSuccess(status);
    }

    private static bool IsSuccess(HttpStatusCode status)
    {
        return (int)status is >= 200 and <= 299;
    }

    private static string Truncate(string body)
    {
        var collapsed = body.ReplaceLineEndings(" ").Trim();

        return collapsed.Length <= 240 ? collapsed : $"{collapsed[..240]}...";
    }

    private async Task<string?> CheckAsync(AuthorizationCase authorizationCase, EndpointRequirement requirement,
        IReadOnlySet<string> permissions, CancellationToken cancellationToken)
    {
        var client = await GetCallerClientAsync(authorizationCase.Caller, cancellationToken);

        using var request = BuildRequest(authorizationCase);
        using var response = await client.SendAsync(request, cancellationToken);

        var status = response.StatusCode;

        if (Satisfies(authorizationCase, requirement, permissions, status))
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return $"{authorizationCase} - {DescribeExpectation(authorizationCase, requirement, permissions)}, but the API answered {(int)status} {status}: {Truncate(body)}";
    }

    private HttpRequestMessage BuildRequest(AuthorizationCase authorizationCase)
    {
        var rowId = authorizationCase.RowIsForeign
            ? ForeignRows.FindRowId(authorizationCase.Operation) ?? MissingRowId
            : MissingRowId;

        var withRow = authorizationCase.Operation.Path.Replace(RowParameter, rowId.ToString(), StringComparison.Ordinal);
        var path = PathParameterPattern().Replace(withRow, UnusedParameterValue);

        var request = new HttpRequestMessage(new HttpMethod(authorizationCase.Operation.Method), path);

        if (authorizationCase.CarriesRequestBody)
        {
            // An empty body reaches authorization and is then refused by validation, which is all a case needs.
            request.Content = JsonContent.Create(new { });
        }

        return request;
    }

    private async Task<Dictionary<AuthorizationCaller, IReadOnlySet<string>>> GetCallerPermissionsAsync(
        CancellationToken cancellationToken)
    {
        var target = await EnsureTenantsAsync(cancellationToken);

        var administrator = await GetPermissionsAsync(target.Owner.UserRowId, cancellationToken);
        var viewer = await GetPermissionsAsync(target.Members["viewer"].UserRowId, cancellationToken);

        return new Dictionary<AuthorizationCaller, IReadOnlySet<string>>
        {
            [AuthorizationCaller.Anonymous] = new HashSet<string>(StringComparer.Ordinal),
            [AuthorizationCaller.Administrator] = administrator,
            [AuthorizationCaller.Viewer] = viewer
        };
    }

    private async Task<IReadOnlySet<string>> GetPermissionsAsync(Guid userRowId, CancellationToken cancellationToken)
    {
        using var scope = CreateScope();

        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        return await permissionService.GetPermissionsAsync(userRowId, cancellationToken);
    }

    private async Task<HttpClient> GetCallerClientAsync(AuthorizationCaller caller, CancellationToken cancellationToken)
    {
        var target = await EnsureTenantsAsync(cancellationToken);

        switch (caller)
        {
            case AuthorizationCaller.Administrator:
                _administratorClient ??= TrackForDisposal(await CreateScenarioClientAsync(target.Owner));

                return _administratorClient;

            case AuthorizationCaller.Viewer:
                _viewerClient ??= TrackForDisposal(await CreateScenarioClientAsync(target.Members["viewer"]));

                return _viewerClient;

            default:
                _anonymousClient ??= TrackForDisposal(CreateClient());

                return _anonymousClient;
        }
    }

    /// <summary>
    /// Reads the contract from a development host, because that is the only environment that publishes it, pointed at
    /// the same database so the inventory describes the application this run exercises.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The documented operations.</returns>
    private async Task<IReadOnlyList<DocumentedOperation>> ReadInventoryAsync(CancellationToken cancellationToken)
    {
        _contractFactory ??= TrackForDisposal(new DevelopmentApiWebApplicationFactory(DbHost, DbPort));

        using var client = _contractFactory.CreateClient();

        return await DocumentedOperations.ReadAsync(client, cancellationToken);
    }

    /// <summary>
    /// Provisions the tenant the run acts in and the tenant whose rows it names, then creates the rows the foreign
    /// cases address.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant the run acts in.</returns>
    private async Task<ProvisionedScenario> EnsureTenantsAsync(CancellationToken cancellationToken)
    {
        if (_target is not null && _foreignRows is not null)
        {
            return _target;
        }

        var targetDefinition = new ScenarioDefinition("matrix-target",
        [
            new ScenarioMember("viewer", Role.Viewer)
        ]);

        var foreignDefinition = new ScenarioDefinition("matrix-foreign",
        [
            new ScenarioMember("viewer", Role.Viewer)
        ]);

        _target = await ProvisionScenarioAsync(targetDefinition, cancellationToken);
        var foreign = await ProvisionScenarioAsync(foreignDefinition, cancellationToken);

        using var foreignClient = await CreateScenarioClientAsync(foreign.Owner);

        var accountRowId = await foreignClient.CreateAccountAsync("Another Tenant Account", 1_000.0d);
        var nextDue = GetSiteLocalDateToday();

        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, nextDue, Frequency.OneTime, 0, 25.0d,
            AccrualPolicy.None, null);

        var expenseRowId = await foreignClient.CreateExpenseAsync(expenseRequest);

        var incomeRequest = IncomeRequestFactory.CreateIncome(accountRowId, nextDue, Frequency.OneTime, 0, 500.0d);
        var incomeRowId = await foreignClient.CreateIncomeAsync(incomeRequest);

        _foreignRows = new ForeignTenantRows(foreign.SiteRowId, foreign.Members["viewer"].UserRowId, accountRowId,
            expenseRowId, incomeRowId);

        return _target;
    }
}
