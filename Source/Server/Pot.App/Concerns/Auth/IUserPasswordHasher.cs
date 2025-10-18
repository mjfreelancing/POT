using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Auth;

public interface IUserPasswordHasher : IPotScopedDependency
{
    string GetHash(UserEntity user, string password);
    bool IsValidPasswordHash(UserEntity user, string password, string passwordHash);
}
