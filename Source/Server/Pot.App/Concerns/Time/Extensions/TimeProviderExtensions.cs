namespace Pot.App.Concerns.Time.Extensions;

public static class TimeProviderExtensions
{
    public static async Task WaitUntilUtcAsync(this ITimeProvider timeProvider, DateTime targetUtc, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Checking more than once in case the system time changes or there is drift that may
            // result in the next occurrence being a matter of seconds later than the expected time.
            var currentUtc = timeProvider.GetUtcDateTimeNow();
            var delayTimespan = targetUtc - currentUtc;

            if (delayTimespan <= TimeSpan.Zero)
            {
                return;
            }

            await timeProvider.DelayAsync(delayTimespan, cancellationToken);
        }
    }
}
