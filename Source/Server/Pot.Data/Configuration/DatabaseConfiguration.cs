namespace Pot.Data.Configuration;

public sealed class DatabaseConfiguration
{
    // Environment variable: DATABASE:NAME
    public required string Name { get; init; }

    // Environment variable: DATABASE:HOST
    public required string Host { get; init; }

    // Environment variable: DATABASE:USERNAME
    public required string Username { get; init; }

    // Environment variable: DATABASE:PASSWORD
    public required string Password { get; init; }

    // Environment variable: DATABASE:Port
    public int Port { get; init; }

    // Environment variable: DATABASE:SSLMode
    public required string SSLMode { get; init; }
}
