using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
/// <summary>
/// Marker interface for the individual pre-update checks composed by <see cref="PreUpdateChecker"/>.
/// </summary>
internal interface IPreUpdateCheck : IPotScopedDependency;

/// <summary>
/// Provides the abstract base for the pre-update checks that validate a user status change.
/// </summary>
internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IPreUpdateCheck
{
}
