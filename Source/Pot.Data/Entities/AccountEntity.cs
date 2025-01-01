#nullable disable           // If enabled, string without [Required] would need to be changed to string?

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index(nameof(Description), IsUnique = true)]
    public sealed class AccountEntity : EntityBase
    {
        [Required]
        [MaxLength(7)]
        public string Bsb { get; set; }

        [Required]
        [MaxLength(20)]
        public string Number { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        public double Balance { get; set; }
        public double Reserved { get; set; }
        public double Allocated { get; set; }
        public double DailyAccrual { get; set; }

        public ICollection<ExpenseEntity> Expenses { get; set; }
    }
}
