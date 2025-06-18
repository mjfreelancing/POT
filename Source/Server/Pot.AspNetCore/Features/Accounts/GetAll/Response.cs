using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.GetAll;

internal sealed class Response : ResponseBase
{
    [Description("The account BSB.")]
    public string Bsb { get; init; }

    [Description("The account number.")]
    public string Number { get; init; }

    [Description("A description of the account.")]
    public string Description { get; init; }

    [Description("The account balance.")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount.")]
    public double Reserved { get; init; }

    [Description("The total amount accrued to pay for future expenses.")]
    public double TotalExpenseAccrued { get; init; }

    [Description("The daily accrual required to meet all future expenses.")]
    public double DailyExpenseAccrual { get; init; }

    [Description("The number of expenses recorded against this account.")]
    public int LinkedExpenses { get; init; }

    [Description("The number of incomes recorded against this account.")]
    public int LinkedIncomes { get; init; }

    [Description("The available balance after consider the Reserved and TotalExpenseAccrued amounts.")]
    public double Available => Balance - Reserved - TotalExpenseAccrued;

    public static Ok<Response[]> Ok(List<Output> accounts)
    {
        var responses = accounts.SelectToArray(account => new Response(account));

        return TypedResults.Ok(responses);
    }

    private Response(Output account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        Etag = account.Etag;
        Bsb = account.Bsb;
        Number = account.Number;
        Description = account.Description;
        Balance = account.Balance;
        Reserved = account.Reserved;
        TotalExpenseAccrued = account.TotalExpenseAccrued;
        DailyExpenseAccrual = account.DailyExpenseAccrual;
        LinkedExpenses = account.LinkedExpenses;
        LinkedIncomes = account.LinkedIncomes;
    }
}
