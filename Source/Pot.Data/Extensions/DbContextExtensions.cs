using AllOverIt.Patterns.ResourceInitialization;
using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Extensions
{
    public static class DbContextExtensions
    {
        public static IDisposable WithAutoTracking(this DbContext dbContext)
        {
            return new Raii(
                () => { dbContext.WithTracking(true); },
                () => { dbContext.WithTracking(false); });
        }

        private static void WithTracking(this DbContext dbContext, bool enabled)
        {
            dbContext.ChangeTracker.QueryTrackingBehavior = enabled
                ? QueryTrackingBehavior.TrackAll
                : QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }
    }
}