using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Annotations
{
    internal sealed class MediumStringAttribute() : MaxLengthAttribute(100)
    {
    }
}
