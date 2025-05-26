using AllOverIt.Patterns.ChainOfResponsibility;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks.Checks;

internal abstract class PreUpdateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, OutputState>
{
}
