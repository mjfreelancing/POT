using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

/// <summary>
/// Marks a validation handler that participates in the account pre-create check chain.
/// </summary>
// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreCreateCheck : IPotScopedDependency;

/// <summary>
/// Provides the base implementation shared by account create validation handlers.
/// </summary>
internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError?>, IPreCreateCheck
{
}
