using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Update.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreUpdateCheck : IPotScopedDependency;

internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ProblemDetailsError>, IPreUpdateCheck
{
}
