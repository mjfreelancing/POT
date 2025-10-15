using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Otp;

public interface IOtpRepository : IGenericRepository<PotDbContext, OneTimePasswordEntity>
{
    Task<List<OneTimePasswordEntity>> GetPendingExpiredAsync(OtpReason? reason, DateTime currentDateUtc, CancellationToken cancellationToken);
    Task<int> CountFailedRequestsForUsernameAsync(OtpReason? reason, string? username, DateTime afterDateUtc, CancellationToken cancellationToken);
    Task<List<OneTimePasswordEntity>> GetActiveRequestsForUsernameAsync(OtpReason? reason, string username, CancellationToken cancellationToken);
    Task<List<OneTimePasswordEntity>> GetRequestsForUsernameAndRefCodeAsync(OtpReason reason, string username, string referenceCode, CancellationToken cancellationToken);
}
