using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Extensions
{
    public static class PotDbContextExtensions
    {
        public static PotDbContext WithTracking(this PotDbContext dbContext, bool enabled)
        {
            dbContext.ChangeTracker.QueryTrackingBehavior = enabled
                ? QueryTrackingBehavior.TrackAll
                : QueryTrackingBehavior.NoTrackingWithIdentityResolution;

            return dbContext;
        }
    }
}