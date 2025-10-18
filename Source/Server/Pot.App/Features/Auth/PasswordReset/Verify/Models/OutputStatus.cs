using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.App.Features.Auth.PasswordReset.Verify.Models;

public sealed class OutputStatus : EnrichedEnum<OutputStatus>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly OutputStatus Success = new(1);
    public static readonly OutputStatus Invalid = new(2);
    public static readonly OutputStatus Expired = new(3);
    public static readonly OutputStatus TooManyAttempts = new(4);

    private OutputStatus(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
