using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete.EntityChecks.Checks;

/// <summary>
/// Marks a validation handler that participates in the account pre-delete check chain.
/// </summary>
// A marker interface so each handler can be dependency injected into the PreDeleteChecker
internal interface IPreDeleteCheck : IPotScopedDependency;

/// <summary>
/// Provides the base implementation shared by account delete validation handlers.
/// </summary>
internal abstract class PreDeleteCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError?>, IPreDeleteCheck
{
}
