using Pot.Data.Entities;

namespace Pot.Data.Repositories.Expenses;

public interface IExpenseRepository : IGenericRepository<PotDbContext, ExpenseEntity>
{
    //Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken);
    //Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken);
    //Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken);
    //Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
    //Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken);
}

internal sealed class ExpenseRepository : GenericRepository<PotDbContext, ExpenseEntity>, IPersistableExpenseRepository
{
    public ExpenseRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    //public Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    return Query().AnyAsync(entity => entity.RowId == id, cancellationToken);
    //}

    //public Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    return Query().SingleAsync(entity => entity.RowId == id, cancellationToken);
    //}

    //public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    return Query().SingleOrDefaultAsync(entity => entity.RowId == id, cancellationToken);
    //}

    //public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    //{
    //    return Query().SingleOrDefaultAsync(entity => entity.Bsb == bsb && entity.Number == number, cancellationToken);
    //}

    //public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
    //{
    //    return Query().AnyAsync(entity => entity.Bsb == bsb && entity.Number == number, cancellationToken);
    //}
}
