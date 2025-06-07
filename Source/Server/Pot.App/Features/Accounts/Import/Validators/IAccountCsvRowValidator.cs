using FluentValidation;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Accounts.Import.Models;

namespace Pot.App.Features.Accounts.Import.Validators;

public interface IAccountCsvRowValidator : IValidator<AccountCsvRow>, IPotScopedDependency
{
}
