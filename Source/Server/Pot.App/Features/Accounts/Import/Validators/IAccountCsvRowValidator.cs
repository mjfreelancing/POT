using FluentValidation;
using Pot.App.Features.Accounts.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Import.Validators;

public interface IAccountCsvRowValidator : IValidator<AccountCsvRow>, IPotScopedDependency
{
}
