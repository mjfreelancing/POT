using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Delete.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the SiteDeleteChecker
/// <summary>
/// Marker interface for the individual pre-delete checks composed by <see cref="SiteDeleteChecker"/>.
/// </summary>
internal interface ISiteDeleteCheck : IPotScopedDependency;

/// <summary>
/// Provides the abstract base for the pre-delete checks that validate a site deletion.
/// </summary>
/// <remarks>
/// <see cref="Order"/> makes the evaluation order explicit. Registration order is not guaranteed, and the
/// confirmation checks have to run before the platform-administrator rule so a caller who mistyped a value is
/// told that, rather than being refused for a reason they cannot act on.
/// </remarks>
internal abstract class SiteDeleteCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, ISiteDeleteCheck
{
    /// <summary>Gets the position of this check in the chain; lower runs first.</summary>
    public abstract int Order { get; }
}
