using FluentValidation;
using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Concerns.Validation.Validators;

public sealed partial class StrongPasswordValidator : AbstractValidator<string>
{
    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
    private static partial Regex SpecialCharacterRegex();

    private static bool HasUppercase(string password) => UppercaseRegex().IsMatch(password);
    private static bool HasLowercase(string password) => LowercaseRegex().IsMatch(password);
    private static bool HasDigit(string password) => DigitRegex().IsMatch(password);
    private static bool HasSpecialCharacter(string password) => SpecialCharacterRegex().IsMatch(password);

    public StrongPasswordValidator()
    {
        RuleFor(password => password)
            .Must(password => !string.IsNullOrWhiteSpace(password) && HasUppercase(password))
            .WithMessage("Password must contain at least one uppercase letter.");

        RuleFor(password => password)
            .Must(password => !string.IsNullOrWhiteSpace(password) && HasLowercase(password))
            .WithMessage("Password must contain at least one lowercase letter.");

        RuleFor(password => password)
            .Must(password => !string.IsNullOrWhiteSpace(password) && HasDigit(password))
            .WithMessage("Password must contain at least one digit.");

        RuleFor(password => password)
            .Must(password => !string.IsNullOrWhiteSpace(password) && HasSpecialCharacter(password))
            .WithMessage("Password must contain at least one special character.");
    }
}
