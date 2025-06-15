using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Create.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreCreateCheck : IPotScopedDependency;

internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ProblemDetailsError?>, IPreCreateCheck
{
}
