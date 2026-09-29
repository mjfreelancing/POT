using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create;

/// <summary>
/// Creates an account for the current site.
/// </summary>
/// <remarks>
/// The account is saved against the site resolved for the current user, and its description must be unique
/// within that site. When the input carries a row identifier (an import) it is preserved so related income and
/// expense records can be linked.
/// </remarks>
public interface ICreateAccountService : IPotScopedDependency
{
    /// <summary>
    /// Creates an account from the supplied details and returns its newly assigned identity.
    /// </summary>
    /// <param name="input">The account details to persist.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the created account's row identifier and etag, or a
    /// failure when an account with the same description already exists for the current site.
    /// </returns>
    Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken);
}
