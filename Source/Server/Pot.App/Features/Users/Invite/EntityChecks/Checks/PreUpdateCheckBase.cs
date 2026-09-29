using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Invite.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
/// <summary>
/// Marker interface for the individual pre-invite checks composed by <see cref="PreUpdateChecker"/>.
/// </summary>
internal interface IPreUpdateCheck : IPotScopedDependency;

/// <summary>
/// Provides the abstract base for the checks that validate a user invitation before it is processed.
/// </summary>
internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IPreUpdateCheck
{
}
