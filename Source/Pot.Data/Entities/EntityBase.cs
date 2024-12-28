#nullable disable           // If enabled, string without [Required] would need to be changed to string?

namespace Pot.Data.Entities
{
    public abstract class EntityBase
    {
        public int Id { get; set; }
        public long Etag { get; set; }
    }
}
