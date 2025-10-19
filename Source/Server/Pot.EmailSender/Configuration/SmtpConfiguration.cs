namespace Pot.EmailSender.Configuration;

public sealed class SmtpConfiguration
{
    public sealed class AuthenticationModel
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }

    public sealed class AddressModel
    {
        public required string Name { get; init; }
        public required string Address { get; init; }
    }

    public required string Host { get; init; }
    public required int Port { get; init; }
    public required bool RequireTls { get; init; }
    public required AuthenticationModel Authentication { get; init; }
    public required AddressModel From { get; init; }
}
