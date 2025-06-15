using FluentValidation;
using Pot.AspNetCore.Features.Expenses.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

public interface IExpenseCsvRowValidator : IValidator<ExpenseCsvRow>, IPotScopedDependency
{
}
