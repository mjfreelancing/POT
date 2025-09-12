using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data;

public interface ICurrentUserDataContext : ICurrentUserContext
{
    Task<UserEntity> GetUserAsync();
}
