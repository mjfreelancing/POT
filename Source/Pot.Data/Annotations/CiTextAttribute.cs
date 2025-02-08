using System.ComponentModel.DataAnnotations.Schema;

namespace Pot.Data.Annotations
{
    internal sealed class CitextAttribute : ColumnAttribute
    {
        public CitextAttribute()
        {
            TypeName = "citext";
        }
    }
}
