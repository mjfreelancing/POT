using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.EntityChecks.Checks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Users.Delete.EntityChecks;

/// <summary>
/// Default implementation of <see cref="IUserDeleteChecker"/>.
/// </summary>
/// <remarks>
/// <para>
/// The checks are ordered deliberately, because they are not independent and the first failure is the only one
/// reported. Ordering is imposed here by <see cref="UserDeleteCheckBase.Order"/> rather than left to injection
/// order, which is not guaranteed.
/// </para>
/// <para>
/// <see cref="Checks.CheckPasswordIsCorrect" /> runs first so a caller who does not know the password learns
/// nothing about the target's protections. <see cref="Checks.CheckTargetIsNotSelf" /> follows, because removing
/// yourself is refused for a different reason and points at the Settings action.
/// <see cref="Checks.CheckTargetIsNotPlatformAdmin" /> then runs before
/// <see cref="Checks.CheckTargetIsNotLastEnabledSiteAdmin" />: a target that is both the last enabled platform
/// administrator and its site's only enabled administrator is refused by the platform rule, which is the one
/// with no in-app remedy, so it must be the message the caller sees.
/// </para>
/// </remarks>
internal sealed class UserDeleteChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError>, IUserDeleteChecker
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates the checker from the available rules, ordered by <see cref="UserDeleteCheckBase.Order"/>.
    /// </summary>
    /// <param name="deleteChecks">The deletion rules registered in the container.</param>
    /// <param name="logger">The logger.</param>
    public UserDeleteChecker(IEnumerable<IUserDeleteCheck> deleteChecks, ILogger<UserDeleteChecker> logger)
        : base(deleteChecks.Cast<UserDeleteCheckBase>().OrderBy(deleteCheck => deleteCheck.Order))
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public Task<ApiDetailError?> CanDeleteAsync(Input input, UserEntity caller, UserEntity target, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            Caller = caller,
            Target = target,
            Password = input.Password,
            PlatformAdminRowIds = input.PlatformAdminRowIds
        };

        return HandleAsync(state, cancellationToken);
    }
}
