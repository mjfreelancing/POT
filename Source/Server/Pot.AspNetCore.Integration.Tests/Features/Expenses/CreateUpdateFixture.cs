using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Expenses;

public class CreateUpdateFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double BillAmount = 70.0d;
    private const int CycleDays = 30;
    private const string StaleEtagMessage = "a stale entity tag must not be accepted";
    private const string DuplicateDescriptionMessage = "a duplicate expense description must not be accepted";
    private const string DuplicateDescriptionError = "The expense description already exists";

    private sealed class ValidationProblem
    {
        public ValidationError[] Errors { get; set; } = [];
    }

    private sealed class ValidationError
    {
        public string? PropertyName { get; set; }
    }

    [Fact]
    public async Task Should_Return_Unauthorized_When_Creating_Without_Authentication()
    {
        using var client = CreateClient();

        var today = GetSiteLocalDateToday();
        var request = ExpenseRequestFactory.CreateBill(Guid.NewGuid(), today, Frequency.Days, CycleDays, BillAmount,
            AccrualPolicy.Automatic, accrualStart: today);
        var response = await client.PostAsJsonAsync("/api/expenses", request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Create_An_Expense_And_Return_It_From_Both_Read_Endpoints()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);
        var expenseRowId = await client.CreateExpenseAsync(request);

        var listed = (await client.GetExpensesAsync()).ShouldHaveSingleItem();

        AssertCreatedExpense(listed, expenseRowId, request, accountRowId);

        var fetched = await client.GetExpenseAsync(expenseRowId);

        AssertCreatedExpense(fetched, expenseRowId, request, accountRowId);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_The_Description_Is_Blank()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today) with
        {
            Description = string.Empty
        };

        var response = await client.PostAsJsonAsync("/api/expenses", request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem.ShouldNotBeNull();
        problem!.Errors.Select(error => error.PropertyName).ShouldContain("Description");
    }

    [Fact]
    public async Task Should_Update_An_Expense()
    {
        const double editedAmount = BillAmount * 2;

        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);
        var expenseRowId = await client.CreateExpenseAsync(request);
        var created = await client.GetExpenseAsync(expenseRowId);
        var updateRequest = request with { Etag = created.Etag, Amount = editedAmount };

        await client.UpdateExpenseAsync(expenseRowId, updateRequest);

        var updated = await client.GetExpenseAsync(expenseRowId);

        updated.Amount.ShouldBe(editedAmount);
        updated.Description.ShouldBe(request.Description, "an update keeps values it did not change");
        updated.AccrualStart.ShouldBe(today);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_The_Etag_Is_Stale()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);
        var expenseRowId = await client.CreateExpenseAsync(request);
        var created = await client.GetExpenseAsync(expenseRowId);
        var staleRequest = request with { Etag = created.Etag - 1 };

        var response = await client.PutAsJsonAsync($"/api/expenses/{expenseRowId}", staleRequest,
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Conflict, StaleEtagMessage);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Creating_An_Expense_With_An_Existing_Description()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        await client.CreateExpenseAsync(request);

        var response = await client.PostAsJsonAsync("/api/expenses", request, TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeEntityConflictAsync("Description", request.Description, DuplicateDescriptionMessage);

        problem.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe(DuplicateDescriptionError);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Updating_An_Expense_To_An_Existing_Description()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var existingRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);
        var requestToUpdate = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        await client.CreateExpenseAsync(existingRequest);

        var expenseRowId = await client.CreateExpenseAsync(requestToUpdate);
        var current = await client.GetExpenseAsync(expenseRowId);
        var duplicateRequest = requestToUpdate with { Etag = current.Etag, Description = existingRequest.Description };

        var response = await client.PutAsJsonAsync($"/api/expenses/{expenseRowId}", duplicateRequest,
            TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeEntityConflictAsync("Description", existingRequest.Description,
            DuplicateDescriptionMessage);

        problem.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe(DuplicateDescriptionError);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Updating_An_Expense_That_Does_Not_Exist()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);
        var response = await client.PutAsJsonAsync($"/api/expenses/{Guid.NewGuid()}", request,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static void AssertCreatedExpense(ExpenseResponse expense, Guid expenseRowId, ExpenseRequest request,
        Guid accountRowId)
    {
        expense.RowId.ShouldBe(expenseRowId);
        expense.Description.ShouldBe(request.Description);
        expense.Amount.ShouldBe(BillAmount);
        expense.NextDue.ShouldBe(DateOnly.ParseExact(request.NextDue, "yyyy-MM-dd"));
        expense.AccrualStart.ShouldBe(DateOnly.ParseExact(request.AccrualStart!, "yyyy-MM-dd"));
        expense.ExcludeFromCalcs.ShouldBeFalse("a new expense is included in calculations");
        expense.Account.ShouldNotBeNull();
        expense.Account!.RowId.ShouldBe(accountRowId);
    }
}
