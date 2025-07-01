using Pot.Data.Entities;

namespace Pot.Data.Extensions;

public static class AccountEntityExtensions
{
    public static double Available(this AccountEntity account)
    {
        return account.Balance - account.Reserved - account.TotalExpenseAccrued;
    }
}
