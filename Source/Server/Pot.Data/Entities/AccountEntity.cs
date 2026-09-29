using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities;

[Index("SiteId", nameof(Description), IsUnique = true)]
public sealed class AccountEntity : EntityBase
{
    [Required]
    [MediumString]
    [Citext]
    public required string Description { get; set; }

    public double Balance { get; set; }
    public double Reserved { get; set; }

    public required SiteEntity Site { get; set; }

    public ICollection<IncomeEntity> Incomes { get; set; } = [];
    public ICollection<ExpenseEntity> Expenses { get; set; } = [];
}
