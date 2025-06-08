using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Get.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

internal sealed class Response : ResponseBase
{
    [Description("The account BSB.")]
    public string Bsb { get; init; } = string.Empty;

    [Description("The account number.")]
    public string Number { get; init; } = string.Empty;

    [Description("A description of the account.")]
    public string Description { get; init; } = string.Empty;

    [Description("The account balance.")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount.")]
    public double Reserved { get; init; }

    [Description("The amount allocated to future expenses.")]
    public double Allocated { get; init; }

    [Description("The daily accrual required to meet all future expenses.")]
    public double DailyAccrual { get; init; }

    [Description("The number of expenses recorded against this account.")]
    public int LinkedExpenses { get; init; }

    [Description("The number of incomes recorded against this account.")]
    public int LinkedIncomes { get; init; }

    [Description("The available balance after consider the Reserved and Allocation amounts.")]
    public double Available => Balance - Reserved - Allocated;

    public static Ok<Response> Ok(Output account)
    {
        return TypedResults.Ok(new Response(account));
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
        Allocated = account.Allocated;
        DailyAccrual = account.DailyAccrual;
        LinkedExpenses = account.LinkedExpenses;
        LinkedIncomes = account.LinkedIncomes;
    }
}
