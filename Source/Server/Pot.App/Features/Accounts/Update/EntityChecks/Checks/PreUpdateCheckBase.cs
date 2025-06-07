using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Update.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreUpdateCheck : IPotScopedDependency;

internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ProblemDetailsError>, IPreUpdateCheck
{
}
