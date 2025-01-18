using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Entities
{
    [Index(nameof(Etag), IsUnique = false)]
    public abstract class EntityBase
    {
        public int Id { get; set; }
        public long Etag { get; set; }
    }
}
