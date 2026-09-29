using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.Upsert.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreUpdateChecker
/// <summary>
/// Marker interface for the individual pre-update checks composed by <see cref="PreUpdateChecker"/>.
/// </summary>
internal interface IPreUpdateCheck : IPotScopedDependency;

/// <summary>
/// Provides the abstract base for the pre-update checks that validate a setting before it is saved.
/// </summary>
internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IPreUpdateCheck
{
}
