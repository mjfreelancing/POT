using System.Security.Cryptography;

namespace Pot.App.Features.Auth;

/// <summary>
/// Generates the six-digit one-time passwords used to verify signup and password reset requests.
/// </summary>
/// <remarks>
/// Codes are produced with a cryptographically secure random number generator and are always formatted as six
/// digits, zero-padded when necessary.
/// </remarks>
public static class OtpGenerator
{
    /// <summary>
    /// Creates a random six-digit one-time password.
    /// </summary>
    /// <returns>A zero-padded six-digit code.</returns>
    public static string Create()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);

        // Ensure uniform distribution across 0-999999
        var value = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return value.ToString("D6");
    }
}
