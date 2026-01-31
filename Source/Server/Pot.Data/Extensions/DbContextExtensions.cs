using AllOverIt.Patterns.ResourceInitialization;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Pot.Data.Extensions;

/// <summary>
/// Provides extension methods for DbContext to manage query tracking behavior with support for nested scopes.
/// 
/// PROBLEM:
/// - By default, this application uses NoTrackingWithIdentityResolution for all queries (for performance)
/// - Some operations need to track entities to persist changes (e.g., updating OTP attempt counts, creating users)
/// - Multiple service methods may need tracking, and they can call each other (nested scopes)
/// - Simple enable/disable tracking doesn't work with nesting - the first disposal would disable tracking for outer scopes
/// 
/// EXAMPLE OF THE NESTING PROBLEM:
/// Service A calls WithTracking() -> enables tracking
///   Service B (called by A) calls WithTracking() -> tracking already enabled
///   Service B disposes -> would disable tracking (breaking Service A's expectation)
/// Service A disposes -> tracking already disabled (entities not saved)
/// 
/// SOLUTION:
/// Reference-counted tracking scopes using ConditionalWeakTable:
/// 
/// 1. REFERENCE COUNTING:
///    - Each DbContext instance gets its own TrackingCounter
///    - Counter increments when entering a WithTracking() scope
///    - Counter decrements when disposing the scope
///    - Tracking is enabled ONLY when count goes from 0 -> 1 (first scope)
///    - Tracking is disabled ONLY when count goes from 1 -> 0 (last scope disposed)
///    - This allows arbitrary nesting depth - tracking stays enabled until ALL scopes are disposed
/// 
/// 2. CONDITIONAL WEAK TABLE:
///    - Static field that lives for application lifetime
///    - Maps DbContext instances to their TrackingCounter
///    - Uses WEAK REFERENCES for keys - doesn't prevent garbage collection
///    - When a DbContext is disposed and GC'd, its entry is automatically removed from the table
///    - Prevents memory leaks - table only contains entries for currently alive DbContext instances
///    - Each HTTP request gets a new scoped DbContext, so each request has its own counter
///    - No manual cleanup required
/// 
/// 3. THREAD SAFETY:
///    - No locks needed because DbContext is NOT thread-safe by design
///    - Each DbContext instance is scoped to a single request/thread
///    - Reference counting only happens within a single thread's call stack (nested using blocks)
///    - The ConditionalWeakTable itself is thread-safe for concurrent access from different threads
/// 
/// USAGE:
/// using (var tracking = _repository.WithTracking())
/// {
///     var entity = await _repository.GetByIdAsync(id);
///     entity.SomeProperty = newValue;
///     await _repository.SaveAsync(); // Changes are tracked and persisted
/// }
/// // Tracking is automatically restored to default (no tracking) when disposed
/// </summary>
public static class DbContextExtensions
{
    private sealed class TrackingCounter
    {
        private int _count;

        public void Increment(DbContext dbContext)
        {
            _count++;

            if (_count == 1)
            {
                // First tracking scope - enable tracking
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        public void Decrement(DbContext dbContext)
        {
            _count--;

            if (_count == 0)
            {
                // Last tracking scope disposed - restore no-tracking
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;
            }
        }
    }

    // Reference count tracking scopes per DbContext instance
    private static readonly ConditionalWeakTable<DbContext, TrackingCounter> TrackingCounters = [];

    public static IDisposable WithAutoTracking(this DbContext dbContext)
    {
        var counter = TrackingCounters.GetOrCreateValue(dbContext);

        return new Raii(
            () => { counter.Increment(dbContext); },
            () => { counter.Decrement(dbContext); });
    }
}
