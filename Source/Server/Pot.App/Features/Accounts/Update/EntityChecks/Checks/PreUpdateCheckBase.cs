using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Update.EntityChecks.Checks;

/// <summary>
/// Marks a validation handler that participates in the account pre-update check chain.
/// </summary>
// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreUpdateCheck : IPotScopedDependency;

/// <summary>
/// Provides the base implementation shared by account update validation handlers.
/// </summary>
internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError>, IPreUpdateCheck
{
}
