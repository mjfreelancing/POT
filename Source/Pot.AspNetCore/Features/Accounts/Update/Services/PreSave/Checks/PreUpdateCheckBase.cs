using AllOverIt.Patterns.ChainOfResponsibility;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreSave.Checks;

internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, OutputState>
{
}
