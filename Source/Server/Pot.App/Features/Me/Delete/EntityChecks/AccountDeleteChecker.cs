using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.EntityChecks.Checks;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Me.Delete.EntityChecks;

/// <summary>
/// Default implementation of <see cref="IAccountDeleteChecker"/>.
/// </summary>
/// <remarks>
/// <para>
/// The checks are ordered deliberately, because they are not independent and the first failure is the only one
/// reported. Ordering is imposed here by <see cref="AccountDeleteCheckBase.Order"/> rather than left to
/// injection order, which is not guaranteed.
/// </para>
/// <para>
/// <see cref="Checks.CheckPasswordIsCorrect" /> runs first so a caller who does not know the password learns
/// nothing about the protections on their account. The typed username is checked next, then
/// <see cref="Checks.CheckIsNotPlatformAdmin" /> before <see cref="Checks.CheckIsNotLastSiteAdmin" />: a caller
/// who is both the last live platform administrator and their site's only enabled administrator is refused by
/// the platform rule, which is the one with no in-app remedy.
/// </para>
/// </remarks>
internal sealed class AccountDeleteChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError>, IAccountDeleteChecker
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates the checker from the available rules, ordered by <see cref="AccountDeleteCheckBase.Order"/>.
    /// </summary>
    /// <param name="deleteChecks">The deletion rules registered in the container.</param>
    /// <param name="logger">The logger.</param>
    public AccountDeleteChecker(IEnumerable<IAccountDeleteCheck> deleteChecks, ILogger<AccountDeleteChecker> logger)
        : base(deleteChecks.Cast<AccountDeleteCheckBase>().OrderBy(deleteCheck => deleteCheck.Order))
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public Task<ApiDetailError?> CanDeleteAsync(Input input, UserEntity caller, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            Caller = caller,
            Username = input.Username,
            Password = input.Password,
            PlatformAdminRowIds = input.PlatformAdminRowIds
        };

        return HandleAsync(state, cancellationToken);
    }
}
