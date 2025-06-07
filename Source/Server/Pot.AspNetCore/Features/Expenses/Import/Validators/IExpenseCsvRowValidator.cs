using FluentValidation;
using Pot.App.Concerns.DependencyInjection;
using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

public interface IExpenseCsvRowValidator : IValidator<ExpenseCsvRow>, IPotScopedDependency
{
}
