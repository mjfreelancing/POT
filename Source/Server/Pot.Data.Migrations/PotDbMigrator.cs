using AllOverIt.EntityFrameworkCore.Migrator;
using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Migrations;

internal sealed class PotDbMigrator : DatabaseMigratorBase<PotDbContext>
{
    public PotDbMigrator(IDbContextFactory<PotDbContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }
}
