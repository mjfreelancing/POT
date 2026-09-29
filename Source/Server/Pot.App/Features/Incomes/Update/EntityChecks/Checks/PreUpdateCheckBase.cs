using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Update.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
/// <summary>
/// Registers an individual income pre-update check for dependency injection.
/// </summary>
internal interface IPreUpdateCheck : IPotScopedDependency;

/// <summary>
/// Base class for the individual checks that validate an income before it is updated.
/// </summary>
internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IPreUpdateCheck
{
}
