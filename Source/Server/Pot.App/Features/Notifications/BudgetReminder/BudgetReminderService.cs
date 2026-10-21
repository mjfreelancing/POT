using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Extensions;
using Pot.App.Features.Expenses.GetAll;
using Pot.App.Features.Incomes.GetAll;
using Pot.App.Features.Settings;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;

namespace Pot.App.Features.Notifications.BudgetReminder;

/// <summary>
/// Default implementation of <see cref="IBudgetReminderService"/>.
/// </summary>
internal sealed class BudgetReminderService : IBudgetReminderService
{
    private readonly IAppContext _appContext;
    private readonly ISettingsService _settingsService;
    private readonly IUserRepository _userRepository;
    private readonly IGetAllIncomesService _incomesService;
    private readonly IGetAllExpensesService _expensesService;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<BudgetReminderService> _logger;

    public BudgetReminderService(IAppContext appContext, ISettingsService settingsService,
        IUserRepository userRepository, IGetAllIncomesService getIncomesService, IGetAllExpensesService getExpensesService,
        ISendEmailChannelWriter sendEmailChannelWriter, ITimeProvider timeProvider, ILogger<BudgetReminderService> logger)
    {
        _appContext = appContext.WhenNotNull();
        _settingsService = settingsService.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _incomesService = getIncomesService.WhenNotNull();
        _expensesService = getExpensesService.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        var currentUtcDateTime = _timeProvider.GetUtcDateTimeNow();
        var currentLocalDateTime = _timeProvider.GetLocalDateTimeNow();
        var currentLocalDate = DateOnly.FromDateTime(currentLocalDateTime.Date);
        var currentLocalHour = currentLocalDateTime.Hour;

        // Cross-tenant by design: the reminder worker scopes the user context per recipient before calling, so
        // this resolves that recipient rather than a signed-in caller.
        var user = _userRepository.GetCurrentUser(true);

        var reminderSettings = await _settingsService.GetEmailBudgetReminderSettingsAsync(cancellationToken);

        if (!reminderSettings.Enabled)
        {
            _logger.LogInformation("Skipping budget reminder email for user {UserRowId} as these reminders are disabled",
                user.RowId);

            return;
        }

        if (reminderSettings.LocalHourTrigger != currentLocalHour)
        {
            _logger.LogInformation("Skipping budget reminder email for user {UserRowId} as the current local hour {CurrentHour} does not match the configured reminder local hour {ReminderHour}",
                user.RowId, currentLocalHour, reminderSettings.LocalHourTrigger);

            return;
        }

        var includedIncomes = await GetIncomesReminder(currentLocalDate, reminderSettings.ReminderDays, cancellationToken);

        var includedExpenses = await GetExpensesReminder(currentLocalDate, reminderSettings.ReminderDays, cancellationToken);

        var reminderInfo = new EmailBudgetReminderInfo
        {
            GeneratedDateTime = _timeProvider.GetLocalDateTimeNow(),

            CurrencySymbol = "$",
            ReminderDays = reminderSettings.ReminderDays,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            LastLoggedIn = user.LastLoggedInUtc.ConvertToLocalDateTime(_appContext)!.Value,

            UserIncomes = includedIncomes.SelectToList(income => new EmailBudgetReminderInfo.IncomeInfo
            {
                Description = income.Description,
                Amount = income.Amount,
                NextDue = income.NextDue,
                DaysDue = income.NextDue.DayNumber - currentLocalDate.DayNumber,
                Note = income.Note
            }),

            UserExpenses = includedExpenses.SelectToList(expense => new EmailBudgetReminderInfo.ExpenseInfo
            {
                Description = expense.Description,
                Amount = expense.Amount,
                NextDue = expense.NextDue,
                DaysDue = expense.NextDue.DayNumber - currentLocalDate.DayNumber,
                Note = expense.Note
            })
        };

        await _sendEmailChannelWriter.SubmitAsync(EmailType.BudgetReminder, reminderInfo, cancellationToken);
    }

    private async Task<List<Expenses.GetAll.Models.Output>> GetExpensesReminder(DateOnly currentLocalDate, int reminderDays, CancellationToken cancellationToken)
    {
        var allExpenses = await _expensesService.GetAllExpensesAsync(cancellationToken);

        return [.. allExpenses
            .Where(expense => !expense.ExcludeFromCalcs)
            .Where(expense => expense.NextDue.DayNumber - currentLocalDate.DayNumber <= reminderDays)
            .OrderBy(expense => expense.NextDue)];
    }

    private async Task<List<Incomes.GetAll.Models.Output>> GetIncomesReminder(DateOnly currentLocalDate, int reminderDays, CancellationToken cancellationToken)
    {
        var allIncomes = await _incomesService.GetAllIncomesAsync(cancellationToken);

        return [.. allIncomes
            .Where(income => !income.ExcludeFromCalcs)
            .Where(income => income.NextDue.DayNumber - currentLocalDate.DayNumber <= reminderDays)
            .OrderBy(income => income.NextDue)];
    }
}
