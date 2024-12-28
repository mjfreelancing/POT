using Pot.Data.Dtos;
using Pot.Data.Entities;

namespace Pot.Data.Extensions
{
    public static class EntityExtensions
    {
        public static AccountDto ToDto(this AccountEntity entity)
        {
            return new AccountDto
            {
                Id = entity.Id,
                ETag = entity.Etag,
                Bsb = entity.Bsb,
                Number = entity.Number,
                Description = entity.Description,
                Balance = entity.Balance,
                Reserved = entity.Reserved,
                Allocated = entity.Allocated,
                DailyAccrual = entity.DailyAccrual
            };
        }

        public static ExpenseDto ToDto(this ExpenseEntity entity)
        {
            return new ExpenseDto
            {
                Id = entity.Id,
                ETag = entity.Etag,
                AccountId = entity.Account.Id,
                Description = entity.Description,
                NextDue = entity.NextDue,
                AccrualStart = entity.AccrualStart,
                Frequency = entity.Frequency,
                FrequencyCount = entity.FrequencyCount,
                Recurring = entity.Recurring,
                Amount = entity.Amount,
                Allocated = entity.Allocated
            };
        }
    }
}