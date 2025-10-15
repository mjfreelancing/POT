using Pot.Data.Entities;

namespace Pot.Data.Repositories.Otp;

public interface IPersistableOtpRepository : IOtpRepository, IPersistableRepository<OneTimePasswordEntity>
{
}