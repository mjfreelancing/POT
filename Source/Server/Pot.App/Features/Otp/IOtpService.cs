using Pot.Shared;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Otp;

public interface IOtpService : IPotScopedDependency
{
    Task<int> UpdateExpiredRequestsAsync(OtpReason? reason, CancellationToken cancellationToken);
    Task<bool> HasReachedRateLimitAsync(OtpReason reason, string username, CancellationToken cancellationToken);
}
