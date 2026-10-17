namespace Pot.Data.Repositories.Sites.Dtos;

/// <summary>
/// The rows removed when a site is deleted.
/// </summary>
/// <param name="Expenses">The number of expenses removed.</param>
/// <param name="Incomes">The number of incomes removed.</param>
/// <param name="Accounts">The number of accounts removed.</param>
/// <param name="Settings">The number of settings removed.</param>
/// <param name="OneTimePasswords">The number of one-time codes removed.</param>
/// <param name="Sessions">The number of authentication sessions removed.</param>
/// <param name="Users">The number of users removed.</param>
public sealed record SiteDeletionCounts(int Expenses, int Incomes, int Accounts, int Settings,
    int OneTimePasswords, int Sessions, int Users);
