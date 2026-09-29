using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.GetAll;

internal sealed class Response : ResponseBase
{
    [Description("A description of the account")]
    public string Description { get; init; }

    [Description("The account balance")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount")]
    public double Reserved { get; init; }

    [Description("The total amount accrued to pay for future expenses")]
    public double TotalExpenseAccrued { get; init; }

    [Description("The total amount owed for expense cycles that are already past due")]
    public double TotalArrears { get; init; }

    [Description("The total committed obligation, being the accrued cycles plus the past-due arrears")]
    public double TotalCommitted { get; init; }

    [Description("The stable daily accrual required for planning based on recurring obligations and active one-time expenses")]
    public double StableExpenseAccrual { get; init; }

    [Description("The number of expenses recorded against this account")]
    public int LinkedExpenses { get; init; }

    [Description("The number of incomes recorded against this account")]
    public int LinkedIncomes { get; init; }

    [Description("The available balance after considering the Reserved and committed amounts")]
    public double Available => Balance - Reserved - TotalCommitted;

    public static Ok<Response[]> Ok(Output[] accounts)
    {
        var responses = accounts.SelectToArray(account => new Response(account));

        return TypedResults.Ok(responses);
    }

    private Response(Output account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        Etag = account.Etag;
        Description = account.Description;
        Balance = account.Balance;
        Reserved = account.Reserved;
        TotalExpenseAccrued = account.TotalExpenseAccrued;
        TotalArrears = account.TotalArrears;
        TotalCommitted = account.TotalCommitted;
        StableExpenseAccrual = account.StableExpenseAccrual;
        LinkedExpenses = account.LinkedExpenses;
        LinkedIncomes = account.LinkedIncomes;
    }
}
