using FluentValidation;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Validators;

public interface IAccountForImportValidator : IValidator<AccountForImport>, IPotScopedDependency
{
}
