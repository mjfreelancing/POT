using Microsoft.AspNetCore.Identity;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Concerns.Auth;

/*
 
 The ASP.NET Core PasswordHasher doesn't produce a hash of a fixed length because the output includes
 not only the hash itself but also the salt, the iteration count, and a format marker. This information
 is necessary for the PasswordHasher to correctly verify the password later. The final result is a
 Base64-encoded string.

 The PasswordHasher uses a format that combines several pieces of data into a single byte array before
 Base64 encoding it. The most common version in recent ASP.NET Core is V3, which has the following structure:

 Format Marker (1 byte): A single byte (0x01) indicating the version of the hashing algorithm.

 PRF (4 bytes): The pseudo-random function used (e.g., HMAC-SHA256).

 Iteration Count (4 bytes): The number of hashing iterations, as an unsigned 32-bit integer.

 Salt Length (4 bytes): The length of the salt, as an unsigned 32-bit integer.

 Salt (16 bytes by default): A random value used to make each hash unique.

 Subkey (32 bytes by default): The actual hash result, also known as the derived key.

 The total size in bytes is: 1 + 4 + 4 + 4 + 16 + 32 = 61 bytes

 This 61-byte array is then Base64 encoded, which increases its length by approximately 33%.
 61 × (4/3) ≈ 81.33

 Since Base64 encoding results in a string, the final length is a whole number, which is typically 81 characters.
 This can vary slightly if the salt or subkey sizes are changed from their defaults.

*/

internal sealed class UserPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    public string GetHash(UserEntity user, string password)
    {
        // The hash is not idempotent. IsValidPasswordHash(), however, will correctly verify two hashes are for the same password.
        return _passwordHasher.HashPassword(user, password);
    }

    public bool IsValidPasswordHash(UserEntity user, string password, string passwordHash)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);

        return verificationResult == PasswordVerificationResult.Success;
    }
}
