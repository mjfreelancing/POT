using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Delete.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the UserDeleteChecker
/// <summary>
/// Marker interface for the individual pre-delete checks composed by <see cref="UserDeleteChecker"/>.
/// </summary>
internal interface IUserDeleteCheck : IPotScopedDependency;

/// <summary>
/// Provides the abstract base for the pre-delete checks that validate a user deletion.
/// </summary>
/// <remarks>
/// <see cref="Order"/> makes the evaluation order explicit. Registration order is not guaranteed, and the
/// rules are not independent: the platform-administrator rule runs before the site-administrator rule so that
/// a target protected by both reports the refusal with no in-app remedy.
/// </remarks>
internal abstract class UserDeleteCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IUserDeleteCheck
{
    /// <summary>Gets the position of this check in the chain; lower runs first.</summary>
    public abstract int Order { get; }
}
