﻿using AllOverIt.Patterns.ChainOfResponsibility;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreCommit.Checks;

internal abstract class PreCommitCheckBase : ChainOfResponsibilityHandlerAsync<InputState, OutputState>
{
}
