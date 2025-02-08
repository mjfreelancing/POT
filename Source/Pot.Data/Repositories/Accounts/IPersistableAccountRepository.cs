using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts;

public interface IPersistableAccountRepository : IAccountRepository, IPersistableRepository<AccountEntity>;
