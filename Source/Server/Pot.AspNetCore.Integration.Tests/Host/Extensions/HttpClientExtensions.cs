using AllOverIt.Assertion;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Host.Extensions;

/// <summary>
/// Seeds and edits the data the integration fixtures assert against, through the public API rather than the
/// database, so every fixture exercises the same create, update and read contracts a client would.
/// </summary>
public static class HttpClientExtensions
{
    private const string AccountsPath = "/api/accounts";
    private const string ExpensesPath = "/api/expenses";
    private const string IncomesPath = "/api/incomes";

    private sealed class IdentifiedResponse
    {
        public Guid RowId { get; set; }
    }

    /// <summary>
    /// Creates an account.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage accounts.</param>
    /// <param name="description">The account description.</param>
    /// <param name="balance">The account balance.</param>
    /// <param name="reserved">The minimum reserved amount.</param>
    /// <returns>The created account's identifier.</returns>
    public static async Task<Guid> CreateAccountAsync(this HttpClient client, string description, double balance,
        double reserved = 0.0d)
    {
        _ = client.WhenNotNull();

        var body = new
        {
            Description = description,
            Balance = balance,
            Reserved = reserved
        };

        var response = await client.PostAsJsonAsync(AccountsPath, body, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Created);

        return await ReadRowIdAsync(response);
    }

    /// <summary>
    /// Creates an expense.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage expenses.</param>
    /// <param name="request">The expense to create.</param>
    /// <returns>The created expense's identifier.</returns>
    public static async Task<Guid> CreateExpenseAsync(this HttpClient client, ExpenseRequest request)
    {
        _ = client.WhenNotNull();
        _ = request.WhenNotNull();

        var response = await client.PostAsJsonAsync(ExpensesPath, request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Created);

        return await ReadRowIdAsync(response);
    }

    /// <summary>
    /// Updates an expense.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage expenses.</param>
    /// <param name="expenseRowId">The expense to update.</param>
    /// <param name="request">The new values, carrying the entity tag the update must match.</param>
    public static async Task UpdateExpenseAsync(this HttpClient client, Guid expenseRowId, ExpenseRequest request)
    {
        _ = client.WhenNotNull();
        _ = request.WhenNotNull();

        var response = await client.PutAsJsonAsync($"{ExpensesPath}/{expenseRowId}", request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    /// <summary>
    /// Renews the given expenses.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage expenses.</param>
    /// <param name="expenseRowIds">The expenses to renew. The endpoint rejects an empty selection.</param>
    /// <param name="asOfDate">The reference date the renewal is measured against.</param>
    /// <param name="mode">Whether to catch up everything overdue, or advance one cycle.</param>
    /// <returns>The response, so a caller can assert either a success or a failure status.</returns>
    public static Task<HttpResponseMessage> RenewExpensesAsync(this HttpClient client, Guid[] expenseRowIds,
        DateOnly asOfDate, RenewalMode mode)
    {
        _ = client.WhenNotNull();
        _ = expenseRowIds.WhenNotNull();

        var body = new
        {
            RowIds = expenseRowIds,
            AsOfDate = asOfDate.ToString("yyyy-MM-dd"),
            Mode = mode.Name
        };

        return client.PostAsJsonAsync($"{ExpensesPath}/renew", body, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Reads every account.
    /// </summary>
    /// <param name="client">A client authenticated with permission to view accounts.</param>
    /// <returns>Every account, in the order the endpoint returns them.</returns>
    public static Task<AccountResponse[]> GetAccountsAsync(this HttpClient client)
    {
        _ = client.WhenNotNull();

        return ReadArrayAsync<AccountResponse>(client, AccountsPath);
    }

    /// <summary>
    /// Reads a single account.
    /// </summary>
    /// <param name="client">A client authenticated with permission to view accounts.</param>
    /// <param name="accountRowId">The account to read.</param>
    /// <returns>The account as the single-item endpoint returns it.</returns>
    public static Task<AccountResponse> GetAccountAsync(this HttpClient client, Guid accountRowId)
    {
        _ = client.WhenNotNull();

        return ReadItemAsync<AccountResponse>(client, $"{AccountsPath}/{accountRowId}");
    }

    /// <summary>
    /// Reads every expense.
    /// </summary>
    /// <param name="client">A client authenticated with permission to view expenses.</param>
    /// <returns>Every expense, in the order the endpoint returns them.</returns>
    public static Task<ExpenseResponse[]> GetExpensesAsync(this HttpClient client)
    {
        _ = client.WhenNotNull();

        return ReadArrayAsync<ExpenseResponse>(client, ExpensesPath);
    }

    /// <summary>
    /// Reads a single expense.
    /// </summary>
    /// <param name="client">A client authenticated with permission to view expenses.</param>
    /// <param name="expenseRowId">The expense to read.</param>
    /// <returns>The expense as the single-item endpoint returns it.</returns>
    public static Task<ExpenseResponse> GetExpenseAsync(this HttpClient client, Guid expenseRowId)
    {
        _ = client.WhenNotNull();

        return ReadItemAsync<ExpenseResponse>(client, $"{ExpensesPath}/{expenseRowId}");
    }

    /// <summary>
    /// Creates an income.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage incomes.</param>
    /// <param name="request">The income to create.</param>
    /// <returns>The created income's identifier.</returns>
    public static async Task<Guid> CreateIncomeAsync(this HttpClient client, IncomeRequest request)
    {
        _ = client.WhenNotNull();
        _ = request.WhenNotNull();

        var response = await client.PostAsJsonAsync(IncomesPath, request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Created);

        return await ReadRowIdAsync(response);
    }

    /// <summary>
    /// Updates an income.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage incomes.</param>
    /// <param name="incomeRowId">The income to update.</param>
    /// <param name="request">The new values, carrying the entity tag the update must match.</param>
    public static async Task UpdateIncomeAsync(this HttpClient client, Guid incomeRowId, IncomeRequest request)
    {
        _ = client.WhenNotNull();
        _ = request.WhenNotNull();

        var response = await client.PutAsJsonAsync($"{IncomesPath}/{incomeRowId}", request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    /// <summary>
    /// Deletes an income.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage incomes.</param>
    /// <param name="incomeRowId">The income to delete.</param>
    /// <returns>The response, so a caller can assert either a success or a failure status.</returns>
    public static Task<HttpResponseMessage> DeleteIncomeAsync(this HttpClient client, Guid incomeRowId)
    {
        _ = client.WhenNotNull();

        return client.DeleteAsync($"{IncomesPath}/{incomeRowId}", TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Reads every income.
    /// </summary>
    /// <param name="client">A client authenticated with permission to view incomes.</param>
    /// <returns>Every income, in the order the endpoint returns them.</returns>
    public static Task<IncomeResponse[]> GetIncomesAsync(this HttpClient client)
    {
        _ = client.WhenNotNull();

        return ReadArrayAsync<IncomeResponse>(client, IncomesPath);
    }

    /// <summary>
    /// Reads a single income.
    /// </summary>
    /// <param name="client">A client authenticated with permission to view incomes.</param>
    /// <param name="incomeRowId">The income to read.</param>
    /// <returns>The income as the single-item endpoint returns it.</returns>
    public static Task<IncomeResponse> GetIncomeAsync(this HttpClient client, Guid incomeRowId)
    {
        _ = client.WhenNotNull();

        return ReadItemAsync<IncomeResponse>(client, $"{IncomesPath}/{incomeRowId}");
    }

    /// <summary>
    /// Renews the given incomes.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage incomes.</param>
    /// <param name="incomeRowIds">The incomes to renew. The endpoint rejects an empty selection.</param>
    /// <param name="asOfDate">The reference date the renewal is measured against.</param>
    /// <param name="mode">Whether to catch up everything overdue, or advance one cycle.</param>
    /// <returns>The response, so a caller can assert either a success or a failure status.</returns>
    public static Task<HttpResponseMessage> RenewIncomesAsync(this HttpClient client, Guid[] incomeRowIds,
        DateOnly asOfDate, RenewalMode mode)
    {
        _ = client.WhenNotNull();
        _ = incomeRowIds.WhenNotNull();

        var body = new
        {
            RowIds = incomeRowIds,
            AsOfDate = asOfDate.ToString("yyyy-MM-dd"),
            Mode = mode.Name
        };

        return client.PostAsJsonAsync($"{IncomesPath}/renew", body, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Toggles the excluded-from-calculations flag on the given incomes.
    /// </summary>
    /// <param name="client">A client authenticated with permission to manage incomes.</param>
    /// <param name="incomeRowIds">The incomes to toggle. The endpoint rejects an empty selection.</param>
    /// <returns>The response, so a caller can assert either a success or a failure status.</returns>
    public static Task<HttpResponseMessage> ToggleExcludeIncomesAsync(this HttpClient client, Guid[] incomeRowIds)
    {
        _ = client.WhenNotNull();
        _ = incomeRowIds.WhenNotNull();

        var body = new
        {
            RowIds = incomeRowIds
        };

        return client.PostAsJsonAsync($"{IncomesPath}/toggleExclude", body, TestContext.Current.CancellationToken);
    }

    private static async Task<Guid> ReadRowIdAsync(HttpResponseMessage response)
    {
        var identified = await response.Content.ReadFromJsonAsync<IdentifiedResponse>(TestContext.Current.CancellationToken);

        identified.ShouldNotBeNull();

        return identified!.RowId;
    }

    private static async Task<T[]> ReadArrayAsync<T>(HttpClient client, string path)
        where T : class
    {
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<T[]>(TestContext.Current.CancellationToken);

        items.ShouldNotBeNull();

        return items!;
    }

    private static async Task<T> ReadItemAsync<T>(HttpClient client, string path)
        where T : class
    {
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var item = await response.Content.ReadFromJsonAsync<T>(TestContext.Current.CancellationToken);

        item.ShouldNotBeNull();

        return item!;
    }
}
