using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index("AccountId", nameof(Description), IsUnique = true)]
    [Index(nameof(NextDue), IsUnique = false)]
    public sealed class ExpenseEntity : EntityBase
    {
        [Required]
        [MediumString]
        [Citext]
        public required string Description { get; set; }

        public DateOnly NextDue { get; set; }
        public DateOnly AccrualStart { get; set; }
        public required ExpenseFrequency Frequency { get; set; }
        public int FrequencyCount { get; set; }
        public bool Recurring { get; set; }
        public double Amount { get; set; }
        public double Allocated { get; set; }

        public AccountEntity? Account { get; set; }
    }
}