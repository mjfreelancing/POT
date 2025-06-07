using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Delete.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreDeleteChecker
internal interface IPreDeleteCheck : IPotScopedDependency;

internal abstract class PreDeleteCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ProblemDetailsError?>, IPreDeleteCheck
{
}
