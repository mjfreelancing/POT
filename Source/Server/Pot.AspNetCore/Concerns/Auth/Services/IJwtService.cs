using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using System.Security.Claims;

namespace Pot.AspNetCore.Concerns.Auth.Services;

public interface IJwtService : IPotSingletonDependency
{
    string CreateAccessToken(UserEntity user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
