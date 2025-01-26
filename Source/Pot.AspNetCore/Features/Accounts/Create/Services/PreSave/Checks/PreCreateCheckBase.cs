using AllOverIt.Patterns.ChainOfResponsibility;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.PreSave.Checks;

internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, OutputState>
{
}
