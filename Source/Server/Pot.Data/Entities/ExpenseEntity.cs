using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Shared;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities;

[Index("AccountId", nameof(Description), IsUnique = true)]
[Index(nameof(NextDue), IsUnique = false)]
public sealed class ExpenseEntity : EntityBase, IHasNextDue
{
    public bool ExcludeFromCalcs { get; set; }

    [Required]
    [MediumString]
    [Citext]
    public required string Description { get; set; }

    public DateOnly AccrualStart { get; set; }
    public DateOnly NextDue { get; set; }                       // As per the user entered, so local timezone is assumed
    public DateOnly? EndDate { get; set; }                      // As per the user entered, so local timezone is assumed
    public required Frequency Frequency { get; set; }
    public int FrequencyCount { get; set; }
    public double Amount { get; set; }
    public double Accrued { get; set; }
    public bool AccruedIsDirty { get; set; } = true;            // See OnModelCreating in PotDbContext
    public DateOnly? LastAccruedUpdate { get; set; }            // Local timezone
    public string? Note { get; set; }

    public required AccountEntity Account { get; set; }
}
