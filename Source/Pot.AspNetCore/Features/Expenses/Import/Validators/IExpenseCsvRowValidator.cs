using FluentValidation;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

public interface IExpenseCsvRowValidator : IValidator<ExpenseCsvRow>, IPotScopedDependency
{
}
