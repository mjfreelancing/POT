using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;
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

    [Description("The amount allocated to future expenses.")]
    public double Allocated { get; init; }

    [Description("The daily accrual required to meet all future expenses.")]
    public double DailyAccrual { get; init; }

    [Description("The available balance after consider the Reserved and Allocation amounts.")]
    public double Available => Balance - Reserved - Allocated;

    public static Ok<Response[]> Ok(List<AccountEntity> accounts)
    {
        var responses = accounts.SelectToArray(account => new Response(account));

        return TypedResults.Ok(responses);
    }

    private Response(AccountEntity account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        ETag = account.Etag;
        Bsb = account.Bsb;
        Number = account.Number;
        Description = account.Description;
        Balance = account.Balance;
        Reserved = account.Reserved;
        Allocated = account.Allocated;
        DailyAccrual = account.DailyAccrual;
    }
}
