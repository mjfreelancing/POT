using System.Security.Cryptography;
using System.Text;

namespace Pot.App.Features.Auth;

public static class PasswordGenerator
{
    // Exclude confusing characters: i, I, l, L, o, O, 0, 1
    private const string LowercaseLetters = "abcdefghjkmnpqrstuvwxyz";
    private const string UppercaseLetters = "ABCDEFGHJKMNPQRSTUVWXYZ";
    private const string Numbers = "23456789";
    private const string SpecialCharacters = "!@#$%^&*()_[]{}<>?";

    private static readonly string AllCharacters = LowercaseLetters + UppercaseLetters + Numbers + SpecialCharacters;

    /// <summary>
    /// Generates a cryptographically secure random password.
    /// </summary>
    /// <param name="length">The desired password length (minimum 4, recommended 12+)</param>
    /// <returns>A random password containing lowercase, uppercase, numbers, and special characters</returns>
    /// <exception cref="ArgumentException">Thrown when length is less than 4</exception>
    public static string Create(int length)
    {
        if (length < 4)
        {
            throw new ArgumentException("Password length must be at least 4 characters", nameof(length));
        }

        using var rng = RandomNumberGenerator.Create();
        var password = new StringBuilder(length);

        // Ensure at least one character from each category
        password.Append(GetRandomCharacter(rng, LowercaseLetters));
        password.Append(GetRandomCharacter(rng, UppercaseLetters));
        password.Append(GetRandomCharacter(rng, Numbers));
        password.Append(GetRandomCharacter(rng, SpecialCharacters));

        // Fill remaining length with random characters from all categories
        for (var i = 4; i < length; i++)
        {
            password.Append(GetRandomCharacter(rng, AllCharacters));
        }

        // Shuffle the password to avoid predictable patterns (first chars always being specific types)
        return Shuffle(rng, password.ToString());
    }

    private static char GetRandomCharacter(RandomNumberGenerator rng, string characterSet)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);

        // Ensure uniform distribution across character set
        var value = BitConverter.ToUInt32(bytes, 0) % characterSet.Length;
        return characterSet[(int)value];
    }

    private static string Shuffle(RandomNumberGenerator rng, string input)
    {
        var chars = input.ToCharArray();
        var n = chars.Length;

        // Fisher-Yates shuffle algorithm
        for (var i = n - 1; i > 0; i--)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);

            var j = (int)(BitConverter.ToUInt32(bytes, 0) % (i + 1));

            // Swap chars[i] and chars[j]
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
