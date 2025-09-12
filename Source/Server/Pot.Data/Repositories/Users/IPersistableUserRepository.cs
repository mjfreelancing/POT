using Pot.Data.Entities;

namespace Pot.Data.Repositories.Users;

public interface IPersistableUserRepository : IUserRepository, IPersistableRepository<UserEntity>;
