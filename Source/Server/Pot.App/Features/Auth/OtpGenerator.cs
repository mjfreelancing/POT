using System.Security.Cryptography;

namespace Pot.App.Features.Auth;

public static class OtpGenerator
{
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
