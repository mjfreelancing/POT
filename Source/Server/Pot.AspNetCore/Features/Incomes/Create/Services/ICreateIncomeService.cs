using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Create.Services;

public interface ICreateIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<IncomeEntity>> CreateIncomeAsync(Request request, CancellationToken cancellationToken);
}
