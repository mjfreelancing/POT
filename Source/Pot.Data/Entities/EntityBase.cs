using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Entities
{
    [Index(nameof(RowId), IsUnique = true)]
    [Index(nameof(Etag), IsUnique = false)]
    public abstract class EntityBase
    {
        public int Id { get; set; }
        public Guid RowId { get; set; }
        public long Etag { get; set; }
    }
}
