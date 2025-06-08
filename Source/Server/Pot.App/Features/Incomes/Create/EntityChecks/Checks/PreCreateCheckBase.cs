using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;

namespace Pot.App.Features.Incomes.Create.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreCreateCheck : IPotScopedDependency;

internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ProblemDetailsError?>, IPreCreateCheck
{
}
