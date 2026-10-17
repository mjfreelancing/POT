using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites.Dtos;
using Pot.Data.Repositories.Users;

namespace Pot.Data.Repositories.Sites;

internal sealed class SiteRepository : PersistableRepository, IPersistableSiteRepository
{
    private readonly IUserRepository _userRepository;

    public SiteRepository(PotDbContext dbContext, IUserRepository userRepository)
        : base(dbContext)
    {
        _userRepository = userRepository.WhenNotNull();
    }

    public SiteEntity GetCurrentSite()
    {
        return _userRepository.GetCurrentUser(true).Site;
    }

    /// <inheritdoc />
    public async Task<SiteDeletionCounts> DeleteCurrentSiteWithDependentsAsync(CancellationToken cancellationToken)
    {
        using (WithTracking())
        {
            // Resolve only the site's identity from the caller, then read the row inside the tracking scope so
            // the whole graph is fixed up and the deletes can be ordered by their relationships.
            var siteId = GetCurrentSite().Id;

            var site = await Set<SiteEntity>()
                .SingleAsync(entity => entity.Id == siteId, cancellationToken);

            // The global query filters already scope these entities to the caller's site, but the sweep states
            // the predicate for itself so its correctness does not depend on the ambient context.
            var expenseCount = await DeleteWhereAsync(
                Set<ExpenseEntity>()
                    .IgnoreQueryFilters()
                    .Include(expense => expense.Account)
                    .Where(expense => expense.Account.Site.Id == site.Id),
                cancellationToken);

            var incomeCount = await DeleteWhereAsync(
                Set<IncomeEntity>()
                    .IgnoreQueryFilters()
                    .Include(income => income.Account)
                    .Where(income => income.Account.Site.Id == site.Id),
                cancellationToken);

            var accountCount = await DeleteWhereAsync(
                Set<AccountEntity>()
                    .IgnoreQueryFilters()
                    .Include(account => account.Site)
                    .Where(account => account.Site.Id == site.Id),
                cancellationToken);

            var settingCount = await DeleteWhereAsync(
                Set<SettingEntity>()
                    .IgnoreQueryFilters()
                    .Include(setting => setting.Site)
                    .Where(setting => setting.Site.Id == site.Id),
                cancellationToken);

            // One-time codes have no site foreign key. A code belongs to the site when it is linked to one of
            // its users, or when it carries one of their usernames (signup codes are written before a user
            // exists).
            var oneTimePasswordCount = await DeleteWhereAsync(
                Set<OneTimePasswordEntity>()
                    .Include(otp => otp.User)
                    .Where(otp =>
                        (otp.User != null && otp.User.Site.Id == site.Id) ||
                        Set<UserEntity>().Any(user => user.Site.Id == site.Id && user.Username == otp.Username)),
                cancellationToken);

            var sessionCount = await DeleteWhereAsync(
                Set<AuthSessionEntity>()
                    .Include(session => session.User)
                    .Where(session => session.User != null && session.User.Site.Id == site.Id),
                cancellationToken);

            var userCount = await DeleteWhereAsync(
                Set<UserEntity>()
                    .Include(user => user.Site)
                    .Where(user => user.Site.Id == site.Id),
                cancellationToken);

            Delete(site);

            await SaveAsync(cancellationToken);

            return new SiteDeletionCounts(expenseCount, incomeCount, accountCount, settingCount,
                oneTimePasswordCount, sessionCount, userCount);
        }
    }
}
