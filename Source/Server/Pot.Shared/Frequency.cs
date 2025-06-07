using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared
{
    public sealed class Frequency : EnrichedEnum<Frequency>
    {
        public static readonly Frequency Days = new(0);
        public static readonly Frequency Weeks = new(1);
        public static readonly Frequency Months = new(2);
        public static readonly Frequency Years = new(3);

        private Frequency(int value, [CallerMemberName] string? name = default)
            : base(value, name!)
        {
        }
    }
}
