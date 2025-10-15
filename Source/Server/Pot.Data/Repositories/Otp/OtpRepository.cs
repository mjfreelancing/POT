using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Otp;

internal sealed class OtpRepository : GenericRepository<PotDbContext, OneTimePasswordEntity>, IPersistableOtpRepository
{
    public OtpRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    // Get all OTPs that have expired and are still Active
    public Task<List<OneTimePasswordEntity>> GetPendingExpiredAsync(OtpReason? reason, DateTime currentDateUtc, CancellationToken cancellationToken)
    {
        var current = reason is null
            ? Current
            : Current.Where(otp => otp.Reason == reason);

        return current
            .Where(otp => otp.Status == OtpStatus.Active && otp.ExpiryUtc <= currentDateUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountFailedRequestsForUsernameAsync(OtpReason? reason, string? username, DateTime afterDateUtc, CancellationToken cancellationToken)
    {
        var current = reason is null
            ? Current
            : Current.Where(otp => otp.Reason == reason);

        return current
            .Where(otp => otp.Username == username && otp.Status == OtpStatus.Failed && otp.CreatedUtc >= afterDateUtc)
            .CountAsync(cancellationToken);
    }

    public Task<List<OneTimePasswordEntity>> GetActiveRequestsForUsernameAsync(OtpReason? reason, string username, CancellationToken cancellationToken)
    {
        var current = reason is null
            ? Current
            : Current.Where(otp => otp.Reason == reason);

        return current
            .Where(otp => otp.Username == username && otp.Status == OtpStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public Task<List<OneTimePasswordEntity>> GetRequestsForUsernameAndRefCodeAsync(OtpReason reason, string username,
        string referenceCode, CancellationToken cancellationToken)
    {
        // There should only be one, but there IS a chance of duplicates
        return Current
            .Where(otp => otp.Reason == reason && otp.Username == username && otp.RefCode == referenceCode)
            .ToListAsync(cancellationToken);
    }
}
