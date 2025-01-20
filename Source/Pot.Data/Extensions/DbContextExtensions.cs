using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Extensions
{
    public static class DbContextExtensions
    {
        public static void WithTracking(this DbContext dbContext, bool enabled)
        {
            dbContext.ChangeTracker.QueryTrackingBehavior = enabled
                ? QueryTrackingBehavior.TrackAll
                : QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }
    }
}