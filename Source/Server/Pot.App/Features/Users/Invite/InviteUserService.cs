using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Auth;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Auth;
using Pot.App.Features.Users.Invite.EntityChecks;
using Pot.App.Features.Users.Invite.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Roles;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.Invite;

internal sealed class InviteUserService : IInviteUserService
{
    private const int InvitationPasswordLength = PasswordGenerator.DefaultLength;

    private readonly IPersistableUserRepository _userRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ILogger _logger;

    public InviteUserService(IPersistableUserRepository userRepository, ISiteRepository siteRepository, IRoleRepository roleRepository,
        IPreUpdateChecker preUpdateChecker, IUserPasswordHasher passwordHasher, ISendEmailChannelWriter sendEmailChannelWriter,
        ILogger<InviteUserService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _siteRepository = siteRepository.WhenNotNull(); ;
        _roleRepository = roleRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _passwordHasher = passwordHasher.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> InviteUserAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.RoleIds });

        var problemDetails = await _preUpdateChecker
            .CanSaveAsync(input, cancellationToken)
            .ConfigureAwait(false);

        if (problemDetails is not null)
        {
            _logger.LogError(problemDetails);

            return EnrichedResult.Fail<bool>(problemDetails);
        }

        using (_userRepository.WithTracking())
        {
            // check if the username is already taken
            var user = await _userRepository
                .GetByUsernameOrDefaultAsync(input.Username, cancellationToken)
                .ConfigureAwait(false);

            if (user is not null)
            {
                var userProblemDetails = ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(input.Username),
                    input.Username,
                    $"The username '{input.Username}' is already in use.");

                _logger.LogError(userProblemDetails);

                return EnrichedResult.Fail<bool>(userProblemDetails);
            }

            var currentSite = _siteRepository.GetCurrentSite();
            var roles = await _roleRepository.GetRolesAsync(input.RoleIds, cancellationToken).ConfigureAwait(false);
            var tempPassword = PasswordGenerator.Create(InvitationPasswordLength);
            var tempPasswordHash = _passwordHasher.GetHash(user, tempPassword);

            user = new UserEntity
            {
                Username = input.Username,
                Email = input.Email,
                DisplayName = input.Username,
                Status = UserStatus.Pending,
                PasswordHash = tempPasswordHash,
                Roles = roles,
                Site = currentSite
            };

            await _userRepository
                .AddAndSaveAsync(user, cancellationToken)
                .ConfigureAwait(false);

            await SendInvitationEmail(user, tempPassword, cancellationToken).ConfigureAwait(false);

            return EnrichedResult.Success(true);
        }
    }

    private ValueTask SendInvitationEmail(UserEntity user, string tempPassword, CancellationToken cancellationToken)
    {
        var emailConfig = new EmailInvitationInfo
        {
            Username = user.Username,
            Email = user.Email,
            TempPassword = tempPassword!,
        };

        return _sendEmailChannelWriter.SubmitAsync(EmailType.Invitation, emailConfig, cancellationToken);
    }
}
