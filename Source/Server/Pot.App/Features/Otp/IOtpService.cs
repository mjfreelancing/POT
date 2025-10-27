using Pot.App.Features.Otp.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Otp;

public interface IOtpService : IPotScopedDependency
{
    Task<int> UpdateExpiredRequestsAsync(OtpReason? reason, CancellationToken cancellationToken);
    Task<bool> HasReachedRateLimitAsync(OtpReason reason, string username, CancellationToken cancellationToken);

    // Used for new signup
    Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, string username, string email, string correlationId, CancellationToken cancellationToken);

    // Used for anything other than new signup, such as password reset
    Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, UserEntity user, string correlationId, CancellationToken cancellationToken);
}
