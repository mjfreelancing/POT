using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index("AccountId", nameof(Description), IsUnique = true)]
    [Index(nameof(NextDue), IsUnique = false)]
    public sealed class IncomeEntity : EntityBase
    {
        [Required]
        [MediumString]
        [Citext]
        public required string Description { get; set; }

        public DateOnly NextDue { get; set; }
        public DateOnly? EndDate { get; set; }
        public required Frequency Frequency { get; set; }
        public int FrequencyCount { get; set; }
        public double Amount { get; set; }

        public required AccountEntity Account { get; set; }
    }
}