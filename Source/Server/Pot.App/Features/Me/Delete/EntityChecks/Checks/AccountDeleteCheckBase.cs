using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Me.Delete.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the AccountDeleteChecker
/// <summary>
/// Marker interface for the individual pre-delete checks composed by <see cref="AccountDeleteChecker"/>.
/// </summary>
internal interface IAccountDeleteCheck : IPotScopedDependency;

/// <summary>
/// Provides the abstract base for the pre-delete checks that validate an account self-deletion.
/// </summary>
/// <remarks>
/// <see cref="Order"/> makes the evaluation order explicit. Registration order is not guaranteed, and the
/// rules are not independent: the platform-administrator rule runs before the site-administrator rule so that
/// a caller protected by both reports the refusal with no in-app remedy.
/// </remarks>
internal abstract class AccountDeleteCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IAccountDeleteCheck
{
    /// <summary>Gets the position of this check in the chain; lower runs first.</summary>
    public abstract int Order { get; }
}
