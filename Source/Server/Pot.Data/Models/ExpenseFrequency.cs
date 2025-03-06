using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Data.Models
{
    public sealed class ExpenseFrequency : EnrichedEnum<ExpenseFrequency>
    {
        public static readonly ExpenseFrequency Days = new(0);
        public static readonly ExpenseFrequency Weeks = new(1);
        public static readonly ExpenseFrequency Months = new(2);
        public static readonly ExpenseFrequency Years = new(3);

        private ExpenseFrequency(int value, [CallerMemberName] string? name = default)
            : base(value, name!)
        {
        }
    }
}
