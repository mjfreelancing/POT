using AllOverIt.Patterns.ChainOfResponsibility;

namespace Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks.Checks;

internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, OutputState>
{
}
