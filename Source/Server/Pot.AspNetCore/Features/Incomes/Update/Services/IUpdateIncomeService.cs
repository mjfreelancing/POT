using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Update.Services;

public interface IUpdateIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<IncomeEntity>> UpdateIncomeAsync(Request request, CancellationToken cancellationToken);
}
