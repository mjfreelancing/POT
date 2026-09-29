using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Create.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
/// <summary>
/// Registers an individual expense pre-create check for dependency injection.
/// </summary>
internal interface IPreCreateCheck : IPotScopedDependency;

/// <summary>
/// Base class for the individual checks that validate an expense before it is created.
/// </summary>
internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError?>, IPreCreateCheck
{
}
