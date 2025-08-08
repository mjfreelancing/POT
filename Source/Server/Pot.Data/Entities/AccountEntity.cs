using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index(nameof(Description), IsUnique = true)]
    [Index(nameof(Bsb), nameof(Number), IsUnique = true)]
    public sealed class AccountEntity : EntityBase
    {
        public bool Disabled { get; set; }

        [Required]
        [MaxLength(7)]
        public required string Bsb { get; set; }

        [Required]
        [MaxLength(20)]
        public required string Number { get; set; }

        [Required]
        [MediumString]
        [Citext]
        public required string Description { get; set; }

        public double Balance { get; set; }
        public double Reserved { get; set; }
        public double TotalExpenseAccrued { get; set; }
        public double DailyExpenseAccrual { get; set; }

        public ICollection<IncomeEntity> Incomes { get; set; } = [];
        public ICollection<ExpenseEntity> Expenses { get; set; } = [];
    }
}
