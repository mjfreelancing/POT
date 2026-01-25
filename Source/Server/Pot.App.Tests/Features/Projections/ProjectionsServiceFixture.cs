using AllOverIt.Extensions;
using AllOverIt.Fixture.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Projections;
using Pot.App.Features.Projections.Models;
using Pot.Data.Repositories.Projections;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.App.Tests.Features.Projections;

public class ProjectionsServiceFixture : PotFixtureBase
{
    private readonly ILogger<ProjectionsService> _logger;

    public ProjectionsServiceFixture()
    {
        CustomizeEnumerations();
        OmitRecursionBehavior();

        _logger = Substitute.For<ILogger<ProjectionsService>>();
    }

    // Helper method to validate that dates are consecutive starting from a specific date
    private static void ValidateConsecutiveDates(IReadOnlyList<DateProjection> projections, DateOnly startDate, int expectedCount)
    {
        projections.Should().HaveCount(expectedCount);

        for (int i = 0; i < projections.Count; i++)
        {
            projections[i].Date.Should().Be(startDate.AddDays(i),
                $"date at index {i} should be {startDate.AddDays(i)}");
        }
    }

    // Helper method to validate empty global projection (no accounts)
    private static void ValidateEmptyGlobalProjection(IReadOnlyList<DateProjection> globalProjections, DateOnly startDate, int expectedCount)
    {
        globalProjections.Should().HaveCount(expectedCount);

        for (int i = 0; i < globalProjections.Count; i++)
        {
            var projection = globalProjections[i];
            var expectedDate = startDate.AddDays(i);

            projection.Date.Should().Be(expectedDate, $"projection at index {i} should have date {expectedDate}");
            projection.Balance.Should().Be(0.0d, $"balance should be 0 for empty projection at {expectedDate}");
            projection.Available.Should().Be(0.0d, $"available should be 0 for empty projection at {expectedDate}");
            projection.DailyAccrual.Should().Be(0.0d, $"daily accrual should be 0 for empty projection at {expectedDate}");
            projection.IncomeReceived.Should().Be(0.0d, $"income received should be 0 for empty projection at {expectedDate}");
            projection.ExpensesPaid.Should().Be(0.0d, $"expenses paid should be 0 for empty projection at {expectedDate}");
            projection.ExpenseItems.Should().BeEmpty($"expense items should be empty for empty projection at {expectedDate}");
            projection.IncomeItems.Should().BeEmpty($"income items should be empty for empty projection at {expectedDate}");
        }
    }

    // Helper method to validate a range of dates with no activity (balance stays constant, no transactions)
    private static void ValidateNoActivityRange(
        IReadOnlyList<DateProjection> projections,
        int startIndex,
        int endIndex,
        double expectedBalance)
    {
        for (int i = startIndex; i <= endIndex; i++)
        {
            var projection = projections[i];
            var dayNumber = i + 1;

            projection.Balance.Should().Be(expectedBalance,
                $"balance on day {dayNumber} ({projection.Date:yyyy-MM-dd}) should remain {expectedBalance}");
            projection.IncomeReceived.Should().Be(0.0d,
                $"no income on day {dayNumber} ({projection.Date:yyyy-MM-dd})");
            projection.ExpensesPaid.Should().Be(0.0d,
                $"no expenses on day {dayNumber} ({projection.Date:yyyy-MM-dd})");
        }
    }

    // Helper method to validate a specific event day (when income/expense occurs)
    private static void ValidateEventDay(
        DateProjection projection,
        DateOnly expectedDate,
        double expectedBalance,
        double? expectedIncomeReceived = null,
        double? expectedExpensesPaid = null,
        string[]? expectedExpenseDescriptions = null,
        string[]? expectedIncomeDescriptions = null)
    {
        projection.Date.Should().Be(expectedDate);
        projection.Balance.Should().Be(expectedBalance);

        if (expectedIncomeReceived.HasValue)
        {
            projection.IncomeReceived.Should().Be(expectedIncomeReceived.Value);
        }

        if (expectedExpensesPaid.HasValue)
        {
            projection.ExpensesPaid.Should().Be(expectedExpensesPaid.Value);
        }

        if (expectedExpenseDescriptions != null)
        {
            projection.ExpenseItems.Select(e => e.Description)
                .Should().BeEquivalentTo(expectedExpenseDescriptions);
        }

        if (expectedIncomeDescriptions != null)
        {
            projection.IncomeItems.Select(i => i.Description)
                .Should().BeEquivalentTo(expectedIncomeDescriptions);
        }
    }

    public class Constructor : ProjectionsServiceFixture
    {
        [Fact]
        public void Should_Throw_When_ProjectionsRepository_Null()
        {
            var timeProvider = Substitute.For<ITimeProvider>();

            Invoking(() =>
            {
                _ = new ProjectionsService(
                    null!,
                    new ExpenseRenewalCalculator(),
                    new IncomeRenewalCalculator(),
                    new AccrueExpenseCalculator(timeProvider),
                    timeProvider,
                    _logger);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("accountRepository");
        }

        [Fact]
        public void Should_Throw_When_ExpenseRenewalCalculator_Null()
        {
            var timeProvider = Substitute.For<ITimeProvider>();

            Invoking(() =>
            {
                _ = new ProjectionsService(
                    Substitute.For<IProjectionsRepository>(),
                    null!,
                    new IncomeRenewalCalculator(),
                    new AccrueExpenseCalculator(timeProvider),
                    timeProvider,
                    _logger);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("expenseRenewalCalculator");
        }

        [Fact]
        public void Should_Throw_When_IncomeRenewalCalculator_Null()
        {
            var timeProvider = Substitute.For<ITimeProvider>();

            Invoking(() =>
            {
                _ = new ProjectionsService(
                    Substitute.For<IProjectionsRepository>(),
                    new ExpenseRenewalCalculator(),
                    null!,
                    new AccrueExpenseCalculator(timeProvider),
                    timeProvider,
                    _logger);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("incomeRenewalCalculator");
        }

        [Fact]
        public void Should_Throw_When_AccrueExpenseCalculator_Null()
        {
            var timeProvider = Substitute.For<ITimeProvider>();

            Invoking(() =>
            {
                _ = new ProjectionsService(
                    Substitute.For<IProjectionsRepository>(),
                    new ExpenseRenewalCalculator(),
                    new IncomeRenewalCalculator(),
                    null!,
                    timeProvider,
                    _logger);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("accrueExpenseCalculator");
        }

        [Fact]
        public void Should_Throw_When_TimeProvider_Null()
        {
            Invoking(() =>
            {
                _ = new ProjectionsService(
                    Substitute.For<IProjectionsRepository>(),
                    new ExpenseRenewalCalculator(),
                    new IncomeRenewalCalculator(),
                    new AccrueExpenseCalculator(Substitute.For<ITimeProvider>()),
                    null!,
                    _logger);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("timeProvider");
        }

        [Fact]
        public void Should_Throw_When_Logger_Null()
        {
            var timeProvider = Substitute.For<ITimeProvider>();

            Invoking(() =>
            {
                _ = new ProjectionsService(
                    Substitute.For<IProjectionsRepository>(),
                    new ExpenseRenewalCalculator(),
                    new IncomeRenewalCalculator(),
                    new AccrueExpenseCalculator(timeProvider),
                    timeProvider,
                    null!);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("logger");
        }
    }

    public class GetFinancialProjectionsAsync : ProjectionsServiceFixture
    {
        private readonly IProjectionsRepository _repository;
        private readonly IExpenseRenewalCalculator _expenseRenewalCalculator;
        private readonly IIncomeRenewalCalculator _incomeRenewalCalculator;
        private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
        private readonly ITimeProvider _timeProvider;
        private readonly ProjectionsService _service;
        private readonly DateOnly _currentDate = new(2025, 1, 15);

        public GetFinancialProjectionsAsync()
        {
            _repository = Substitute.For<IProjectionsRepository>();
            _timeProvider = Substitute.For<ITimeProvider>();

            // Using real calculators since they contain the core logic for projections
            // and we want to test the integration of the service with these calculators.
            _expenseRenewalCalculator = new ExpenseRenewalCalculator();
            _incomeRenewalCalculator = new IncomeRenewalCalculator();
            _accrueExpenseCalculator = new AccrueExpenseCalculator(_timeProvider);

            _service = new ProjectionsService(
                _repository,
                _expenseRenewalCalculator,
                _incomeRenewalCalculator,
                _accrueExpenseCalculator,
                _timeProvider,
                _logger);

            _timeProvider.GetLocalDateNow().Returns(_currentDate);
        }

        [Fact]
        public async Task Should_Throw_When_StartDate_Before_Current_Date()
        {
            var options = new ProjectionOptions
            {
                StartDate = _currentDate.AddDays(-1),
                DaysForecast = 30
            };

            _repository
                .GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                .Returns([]);

            await Invoking(async () =>
            {
                await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);
            })
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Projections cannot start earlier than today");
        }

        [Fact]
        public async Task Should_Return_Empty_Projections_When_No_Accounts()
        {
            var options = new ProjectionOptions
            {
                StartDate = _currentDate,
                DaysForecast = 30
            };

            _repository
                .GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                .Returns([]);

            var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Accounts.Should().BeEmpty();

            // Validate that all 30 global projection dates are properly initialized with zero values
            ValidateEmptyGlobalProjection(result.Value.Global, _currentDate, 30);
        }

        [Fact]
        public async Task Should_Project_Single_Account_With_No_Expenses_Or_Incomes()
        {
            var account = EntityFactory.CreateAccount("Checking", 1000.0d);

            var options = new ProjectionOptions
            {
                StartDate = _currentDate,
                DaysForecast = 30
            };

            _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                .Returns([account]);

            var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Accounts.Should().HaveCount(1);

            var accountProjection = result.Value.Accounts[0];

            // Validate all dates are consecutive
            ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

            // Validate all dates have the same balance with no transactions
            accountProjection.Dates.Should().AllSatisfy(date =>
            {
                date.Balance.Should().Be(1000.0d);
                date.Available.Should().Be(1000.0d);
                date.DailyAccrual.Should().Be(0.0d);
                date.IncomeReceived.Should().Be(0.0d);
                date.ExpensesPaid.Should().Be(0.0d);
                date.ExpenseItems.Should().BeEmpty();
                date.IncomeItems.Should().BeEmpty();
            });
        }

        // ===== 1 MONTH PROJECTIONS =====
        public class OneMonthProjections : GetFinancialProjectionsAsync
        {
            [Fact]
            public async Task Should_Project_Single_Monthly_Expense_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 1000.0d);
                var expense = EntityFactory.CreateExpense(account, false, "Rent", 500.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                account.Expenses.Add(expense);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-15 (Jan 15 - Jan 30): Before expense, balance stays at 1000
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 1000.0d);

                // Day 16 (Jan 31): Expense paid
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 500.0d,
                    expectedExpensesPaid: 500.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // Days 17-29 (Feb 1 - Feb 13): After expense, balance stays at 500
                ValidateNoActivityRange(accountProjection.Dates, 17, 29, expectedBalance: 500.0d);
            }

            [Fact]
            public async Task Should_Project_Two_Monthly_Expenses_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 2000.0d);
                var rent = EntityFactory.CreateExpense(account, false, "Rent", 800.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var utilities = EntityFactory.CreateExpense(account, false, "Utilities", 150.0d, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                account.Expenses.Add(rent);
                account.Expenses.Add(utilities);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-9 (Jan 15 - Jan 24): Before any expenses
                ValidateNoActivityRange(accountProjection.Dates, 0, 9, expectedBalance: 2000.0d);

                // Day 10 (Jan 25): Utilities paid
                ValidateEventDay(
                    accountProjection.Dates[10],
                    expectedDate: new DateOnly(2025, 1, 25),
                    expectedBalance: 1850.0d,
                    expectedExpensesPaid: 150.0d,
                    expectedExpenseDescriptions: ["Utilities"]);

                // Days 11-15 (Jan 26 - Jan 30): After utilities, before rent
                ValidateNoActivityRange(accountProjection.Dates, 11, 15, expectedBalance: 1850.0d);

                // Day 16 (Jan 31): Rent paid
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 1050.0d,
                    expectedExpensesPaid: 800.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // Days 17-29 (Feb 1 - Feb 13): After both expenses
                ValidateNoActivityRange(accountProjection.Dates, 17, 29, expectedBalance: 1050.0d);
            }

            [Fact]
            public async Task Should_Project_Three_Mixed_Frequency_Expenses_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 3000.0d);
                var rent = EntityFactory.CreateExpense(account, false, "Rent", 1000.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var groceries = EntityFactory.CreateExpense(account, false, "Groceries", 100.0d, "2025-01-01", "2025-01-20", null, Frequency.Weeks, 1);
                var subscription = EntityFactory.CreateExpense(account, false, "Subscription", 50.0d, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                account.Expenses.Add(rent);
                account.Expenses.Add(groceries);
                account.Expenses.Add(subscription);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-4 (Jan 15 - Jan 19): Before any expenses
                ValidateNoActivityRange(accountProjection.Dates, 0, 4, expectedBalance: 3000.0d);

                // Day 5 (Jan 20): Groceries paid
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 2900.0d,
                    expectedExpensesPaid: 100.0d,
                    expectedExpenseDescriptions: ["Groceries"]);

                // Days 6-9 (Jan 21 - Jan 24): After groceries, before subscription
                ValidateNoActivityRange(accountProjection.Dates, 6, 9, expectedBalance: 2900.0d);

                // Day 10 (Jan 25): Subscription paid
                ValidateEventDay(
                    accountProjection.Dates[10],
                    expectedDate: new DateOnly(2025, 1, 25),
                    expectedBalance: 2850.0d,
                    expectedExpensesPaid: 50.0d,
                    expectedExpenseDescriptions: ["Subscription"]);

                // Day 11 (Jan 26): No activity
                ValidateNoActivityRange(accountProjection.Dates, 11, 11, expectedBalance: 2850.0d);

                // Day 12 (Jan 27): Weekly groceries again
                ValidateEventDay(
                    accountProjection.Dates[12],
                    expectedDate: new DateOnly(2025, 1, 27),
                    expectedBalance: 2750.0d,
                    expectedExpensesPaid: 100.0d,
                    expectedExpenseDescriptions: ["Groceries"]);

                // Days 13-15 (Jan 28 - Jan 30): After second groceries, before rent
                ValidateNoActivityRange(accountProjection.Dates, 13, 15, expectedBalance: 2750.0d);

                // Day 16 (Jan 31): Rent paid
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 1750.0d,
                    expectedExpensesPaid: 1000.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // Days 17-18 (Feb 1 - Feb 2): After rent, before next weekly groceries
                ValidateNoActivityRange(accountProjection.Dates, 17, 18, expectedBalance: 1750.0d);

                // Day 19 (Feb 3): Weekly groceries continues
                ValidateEventDay(
                    accountProjection.Dates[19],
                    expectedDate: new DateOnly(2025, 2, 3),
                    expectedBalance: 1650.0d,
                    expectedExpensesPaid: 100.0d,
                    expectedExpenseDescriptions: ["Groceries"]);

                // Days 20-25 (Feb 4 - Feb 9): After third groceries, before fourth
                ValidateNoActivityRange(accountProjection.Dates, 20, 25, expectedBalance: 1650.0d);

                // Day 26 (Feb 10): Weekly groceries continues
                ValidateEventDay(
                    accountProjection.Dates[26],
                    expectedDate: new DateOnly(2025, 2, 10),
                    expectedBalance: 1550.0d,
                    expectedExpensesPaid: 100.0d,
                    expectedExpenseDescriptions: ["Groceries"]);

                // Days 27-29 (Feb 11 - Feb 13): After fourth groceries
                ValidateNoActivityRange(accountProjection.Dates, 27, 29, expectedBalance: 1550.0d);
            }

            [Fact]
            public async Task Should_Project_One_Off_Expense_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 1000.0d);
                var oneOff = EntityFactory.CreateExpense(account, false, "One-Time Purchase", 200.0d, "2025-01-01", "2025-01-25", null, Frequency.OneTime, 1);
                account.Expenses.Add(oneOff);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-9 (Jan 15 - Jan 24): Before one-off expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 9, expectedBalance: 1000.0d);

                // Day 10 (Jan 25): One-off expense paid
                ValidateEventDay(
                    accountProjection.Dates[10],
                    expectedDate: new DateOnly(2025, 1, 25),
                    expectedBalance: 800.0d,
                    expectedExpensesPaid: 200.0d,
                    expectedExpenseDescriptions: ["One-Time Purchase"]);

                // Days 11-29 (Jan 26 - Feb 13): After one-off expense (should not repeat)
                ValidateNoActivityRange(accountProjection.Dates, 11, 29, expectedBalance: 800.0d);
            }

            [Fact]
            public async Task Should_Project_Expense_With_End_Date_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 1500.0d);
                var limited = EntityFactory.CreateExpense(account, false, "Limited Subscription", 50.0d, "2025-01-01", "2025-01-20", "2025-01-27", Frequency.Weeks, 1);
                account.Expenses.Add(limited);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-4 (Jan 15 - Jan 19): Before first occurrence
                ValidateNoActivityRange(accountProjection.Dates, 0, 4, expectedBalance: 1500.0d);

                // Day 5 (Jan 20): First expense occurrence
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 1450.0d,
                    expectedExpensesPaid: 50.0d,
                    expectedExpenseDescriptions: ["Limited Subscription"]);

                // Days 6-11 (Jan 21 - Jan 26): Between first and second occurrence
                ValidateNoActivityRange(accountProjection.Dates, 6, 11, expectedBalance: 1450.0d);

                // Day 12 (Jan 27): Second expense occurrence (last before end date)
                ValidateEventDay(
                    accountProjection.Dates[12],
                    expectedDate: new DateOnly(2025, 1, 27),
                    expectedBalance: 1400.0d,
                    expectedExpensesPaid: 50.0d,
                    expectedExpenseDescriptions: ["Limited Subscription"]);

                // Days 13-29 (Jan 28 - Feb 13): After end date, no more occurrences
                ValidateNoActivityRange(accountProjection.Dates, 13, 29, expectedBalance: 1400.0d);
            }

            [Fact]
            public async Task Should_Project_Income_And_Expense_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 500.0d);
                var salary = EntityFactory.CreateIncome(account, false, "Salary", 3000.0d, "2025-01-31", null, Frequency.Months, 1);
                var rent = EntityFactory.CreateExpense(account, false, "Rent", 1200.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                account.Incomes.Add(salary);
                account.Expenses.Add(rent);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-15 (Jan 15 - Jan 30): Before income and expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 500.0d);

                // Day 16 (Jan 31): Both income and expense on same day
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 2300.0d,
                    expectedIncomeReceived: 3000.0d,
                    expectedExpensesPaid: 1200.0d,
                    expectedIncomeDescriptions: ["Salary"],
                    expectedExpenseDescriptions: ["Rent"]);

                // Days 17-29 (Feb 1 - Feb 13): After income and expense
                ValidateNoActivityRange(accountProjection.Dates, 17, 29, expectedBalance: 2300.0d);
            }

            [Fact]
            public async Task Should_Exclude_Expenses_Marked_As_Excluded_For_One_Month()
            {
                var account = EntityFactory.CreateAccount("Checking", 1000.0d);
                var included = EntityFactory.CreateExpense(account, false, "Included", 100.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                var excluded = EntityFactory.CreateExpense(account, true, "Excluded", 500.0d, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                account.Expenses.Add(included);
                account.Expenses.Add(excluded);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-4 (Jan 15 - Jan 19): Before any expenses
                ValidateNoActivityRange(accountProjection.Dates, 0, 4, expectedBalance: 1000.0d);

                // Day 5 (Jan 20): Included expense paid
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 900.0d,
                    expectedExpensesPaid: 100.0d,
                    expectedExpenseDescriptions: ["Included"]);

                // Days 6-9 (Jan 21 - Jan 24): After included, before excluded
                ValidateNoActivityRange(accountProjection.Dates, 6, 9, expectedBalance: 900.0d);

                // Day 10 (Jan 25): Excluded expense appears (ExcludeFromCalcs affects accruals, not projections)
                ValidateEventDay(
                    accountProjection.Dates[10],
                    expectedDate: new DateOnly(2025, 1, 25),
                    expectedBalance: 400.0d,
                    expectedExpensesPaid: 500.0d,
                    expectedExpenseDescriptions: ["Excluded"]);

                // Days 11-29 (Jan 26 - Feb 13): After both expenses
                ValidateNoActivityRange(accountProjection.Dates, 11, 29, expectedBalance: 400.0d);
            }
        }

        // ===== 3 MONTH PROJECTIONS =====
        public class ThreeMonthProjections : GetFinancialProjectionsAsync
        {
            [Fact]
            public async Task Should_Project_Bi_Monthly_Expense_For_Three_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 5000.0d);
                var insurance = EntityFactory.CreateExpense(account, false, "Insurance", 200.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 2);
                account.Expenses.Add(insurance);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 90
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Apr 14)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 90);

                // Days 0-15 (Jan 15 - Jan 30): Before first expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 5000.0d);

                // Day 16 (Jan 31): First bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 4800.0d,
                    expectedExpensesPaid: 200.0d,
                    expectedExpenseDescriptions: ["Insurance"]);

                // Days 17-74 (Feb 1 - Mar 30): Between expenses (all of February + most of March, no expenses)
                ValidateNoActivityRange(accountProjection.Dates, 17, 74, expectedBalance: 4800.0d);

                // Day 75 (Mar 31): Second bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[75],
                    expectedDate: new DateOnly(2025, 3, 31),
                    expectedBalance: 4600.0d,
                    expectedExpensesPaid: 200.0d,
                    expectedExpenseDescriptions: ["Insurance"]);

                // Days 76-89 (Apr 1 - Apr 14): After second expense
                ValidateNoActivityRange(accountProjection.Dates, 76, 89, expectedBalance: 4600.0d);
            }

            [Fact]
            public async Task Should_Project_Quarterly_Expense_For_Three_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 10000.0d);
                var quarterly = EntityFactory.CreateExpense(account, false, "Quarterly Tax", 1500.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 3);
                account.Expenses.Add(quarterly);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 90
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Apr 14)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 90);

                // Days 0-15 (Jan 15 - Jan 30): Before quarterly expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 10000.0d);

                // Day 16 (Jan 31): Quarterly expense occurs
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 8500.0d,
                    expectedExpensesPaid: 1500.0d,
                    expectedExpenseDescriptions: ["Quarterly Tax"]);

                // Days 17-89 (Feb 1 - Apr 14): After expense, no repeat (next would be Apr 30)
                ValidateNoActivityRange(accountProjection.Dates, 17, 89, expectedBalance: 8500.0d);
            }

            [Fact]
            public async Task Should_Project_Weekly_Expense_For_Three_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 2000.0d);
                var weekly = EntityFactory.CreateExpense(account, false, "Weekly Groceries", 75.0d, "2025-01-01", "2025-01-20", null, Frequency.Weeks, 1);
                account.Expenses.Add(weekly);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 90
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Apr 14)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 90);

                // Validate weekly pattern: Jan 20, 27, Feb 3, 10, 17, 24, Mar 3, 10, 17, 24, 31, Apr 7, 14
                var expectedWeeklyDates = new[]
                {
                    new DateOnly(2025, 1, 20), new DateOnly(2025, 1, 27),
                    new DateOnly(2025, 2, 3), new DateOnly(2025, 2, 10), new DateOnly(2025, 2, 17), new DateOnly(2025, 2, 24),
                    new DateOnly(2025, 3, 3), new DateOnly(2025, 3, 10), new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 24), new DateOnly(2025, 3, 31),
                    new DateOnly(2025, 4, 7), new DateOnly(2025, 4, 14)
                };

                // Validate all expense occurrences
                var occurrences = accountProjection.Dates.Where(d => d.ExpensesPaid > 0).ToList();
                occurrences.Should().HaveCount(expectedWeeklyDates.Length);

                for (int i = 0; i < expectedWeeklyDates.Length; i++)
                {
                    occurrences[i].Date.Should().Be(expectedWeeklyDates[i]);
                    occurrences[i].ExpensesPaid.Should().Be(75.0d);
                    occurrences[i].ExpenseItems.Should().ContainSingle(e => e.Description == "Weekly Groceries");
                }

                // Validate final balance after all weekly expenses
                var finalBalance = 2000.0d - (expectedWeeklyDates.Length * 75.0d);
                accountProjection.Dates[89].Balance.Should().Be(finalBalance);
            }

            [Fact]
            public async Task Should_Project_Expense_Ending_Mid_Period_For_Three_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 3000.0d);
                var limited = EntityFactory.CreateExpense(account, false, "Limited Contract", 300.0d, "2025-01-01", "2025-01-31", "2025-02-28", Frequency.Months, 1);
                account.Expenses.Add(limited);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 90
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Apr 14)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 90);

                // Days 0-15 (Jan 15 - Jan 30): Before first occurrence
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 3000.0d);

                // Day 16 (Jan 31): First monthly expense
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 2700.0d,
                    expectedExpensesPaid: 300.0d,
                    expectedExpenseDescriptions: ["Limited Contract"]);

                // Days 17-43 (Feb 1 - Feb 27): Between expenses
                ValidateNoActivityRange(accountProjection.Dates, 17, 43, expectedBalance: 2700.0d);

                // Day 44 (Feb 28): Second monthly expense (last occurrence before end date)
                ValidateEventDay(
                    accountProjection.Dates[44],
                    expectedDate: new DateOnly(2025, 2, 28),
                    expectedBalance: 2400.0d,
                    expectedExpensesPaid: 300.0d,
                    expectedExpenseDescriptions: ["Limited Contract"]);

                // Days 45-89 (Mar 1 - Apr 14): After end date, no more occurrences
                ValidateNoActivityRange(accountProjection.Dates, 45, 89, expectedBalance: 2400.0d);
            }

            [Fact]
            public async Task Should_Project_Multiple_Accounts_For_Three_Months()
            {
                var checking = EntityFactory.CreateAccount("Checking", 2000.0d);
                var savings = EntityFactory.CreateAccount("Savings", 10000.0d);

                var rent = EntityFactory.CreateExpense(checking, false, "Rent", 1000.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var transfer = EntityFactory.CreateExpense(savings, false, "Investment", 500.0d, "2025-01-01", "2025-01-15", null, Frequency.Months, 1);

                checking.Expenses.Add(rent);
                savings.Expenses.Add(transfer);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 90
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([checking, savings]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                result.Value!.Accounts.Should().HaveCount(2);

                var checkingProjection = result.Value.Accounts.First(a => a.Description == "Checking");
                var savingsProjection = result.Value.Accounts.First(a => a.Description == "Savings");

                // Validate checking account projections (Jan 15 - Apr 14)
                ValidateConsecutiveDates(checkingProjection.Dates, _currentDate, 90);

                // Checking: Days 0-15 (Jan 15 - Jan 30): Before rent
                ValidateNoActivityRange(checkingProjection.Dates, 0, 15, expectedBalance: 2000.0d);

                // Checking: Day 16 (Jan 31): First rent payment
                ValidateEventDay(
                    checkingProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 1000.0d,
                    expectedExpensesPaid: 1000.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // Checking: Days 17-43 (Feb 1 - Feb 27): After first rent, before second
                ValidateNoActivityRange(checkingProjection.Dates, 17, 43, expectedBalance: 1000.0d);

                // Checking: Day 44 (Feb 28): Second rent payment
                ValidateEventDay(
                    checkingProjection.Dates[44],
                    expectedDate: new DateOnly(2025, 2, 28),
                    expectedBalance: 0.0d,
                    expectedExpensesPaid: 1000.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // Checking: Days 45-71 (Mar 1 - Mar 27): After second rent, before third
                ValidateNoActivityRange(checkingProjection.Dates, 45, 71, expectedBalance: 0.0d);

                // Checking: Day 72 (Mar 28): Third rent payment (goes negative)
                ValidateEventDay(
                    checkingProjection.Dates[72],
                    expectedDate: new DateOnly(2025, 3, 28),
                    expectedBalance: -1000.0d,
                    expectedExpensesPaid: 1000.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // Checking: Days 73-89 (Mar 29 - Apr 14): After third rent
                ValidateNoActivityRange(checkingProjection.Dates, 73, 89, expectedBalance: -1000.0d);

                // Validate savings account projections (Jan 15 - Apr 14)
                ValidateConsecutiveDates(savingsProjection.Dates, _currentDate, 90);

                // Savings: Day 0 (Jan 15): Investment paid on start date
                ValidateEventDay(
                    savingsProjection.Dates[0],
                    expectedDate: _currentDate,
                    expectedBalance: 9500.0d,
                    expectedExpensesPaid: 500.0d,
                    expectedExpenseDescriptions: ["Investment"]);

                // Savings: Days 1-30 (Jan 16 - Feb 14): After first investment, before second
                ValidateNoActivityRange(savingsProjection.Dates, 1, 30, expectedBalance: 9500.0d);

                // Savings: Day 31 (Feb 15): Second monthly investment
                ValidateEventDay(
                    savingsProjection.Dates[31],
                    expectedDate: new DateOnly(2025, 2, 15),
                    expectedBalance: 9000.0d,
                    expectedExpensesPaid: 500.0d,
                    expectedExpenseDescriptions: ["Investment"]);

                // Savings: Days 32-58 (Feb 16 - Mar 14): After second investment, before third
                ValidateNoActivityRange(savingsProjection.Dates, 32, 58, expectedBalance: 9000.0d);

                // Savings: Day 59 (Mar 15): Third monthly investment
                ValidateEventDay(
                    savingsProjection.Dates[59],
                    expectedDate: new DateOnly(2025, 3, 15),
                    expectedBalance: 8500.0d,
                    expectedExpensesPaid: 500.0d,
                    expectedExpenseDescriptions: ["Investment"]);

                // Savings: Days 60-89 (Mar 16 - Apr 14): After third investment
                ValidateNoActivityRange(savingsProjection.Dates, 60, 89, expectedBalance: 8500.0d);

                // Verify global aggregation for all 90 days
                var globalProjections = result.Value.Global;
                ValidateConsecutiveDates(globalProjections, _currentDate, 90);

                // Global: Day 0 (Jan 15): Investment paid on start date
                globalProjections[0].Balance.Should().Be(11500.0d); // 2000 + 10000 - 500

                // Global: Day 16 (Jan 31): First rent paid
                globalProjections[16].Balance.Should().Be(10500.0d); // 11500 - 1000

                // Global: Day 31 (Feb 15): Second investment paid
                globalProjections[31].Balance.Should().Be(10000.0d); // 10500 - 500

                // Global: Day 44 (Feb 28): Second rent paid
                globalProjections[44].Balance.Should().Be(9000.0d); // 10000 - 1000

                // Global: Day 59 (Mar 15): Third investment paid
                globalProjections[59].Balance.Should().Be(8500.0d); // 9000 - 500

                // Global: Day 72 (Mar 28): Third rent paid
                globalProjections[72].Balance.Should().Be(7500.0d); // 8500 - 1000

                // Global: Day 89 (Apr 14): Final balance
                globalProjections[89].Balance.Should().Be(7500.0d); // No changes after Mar 28
            }
        }

        // ===== 6 MONTH PROJECTIONS =====
        public class SixMonthProjections : GetFinancialProjectionsAsync
        {
            [Fact]
            public async Task Should_Project_Semi_Annual_Expense_For_Six_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 15000.0d);
                var semiAnnual = EntityFactory.CreateExpense(account, false, "Insurance Premium", 600.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 6);
                account.Expenses.Add(semiAnnual);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 180
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jul 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 180);

                // Days 0-15 (Jan 15 - Jan 30): Before semi-annual expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 15000.0d);

                // Day 16 (Jan 31): Semi-annual expense occurs
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 14400.0d,
                    expectedExpensesPaid: 600.0d,
                    expectedExpenseDescriptions: ["Insurance Premium"]);

                // Days 17-179 (Feb 1 - Jul 13): After expense, no repeat (next would be Jul 31)
                ValidateNoActivityRange(accountProjection.Dates, 17, 179, expectedBalance: 14400.0d);
            }

            [Fact]
            public async Task Should_Project_Monthly_Expenses_For_Six_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 20000.0d);
                var rent = EntityFactory.CreateExpense(account, false, "Rent", 1500.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                account.Expenses.Add(rent);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 180
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jul 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 180);

                // Validate 6 monthly rent occurrences: Jan 31, Feb 28, Mar 28, Apr 28, May 28, Jun 28
                var expectedDates = new[]
                {
                    new DateOnly(2025, 1, 31),
                    new DateOnly(2025, 2, 28),
                    new DateOnly(2025, 3, 28),
                    new DateOnly(2025, 4, 28),
                    new DateOnly(2025, 5, 28),
                    new DateOnly(2025, 6, 28)
                };

                var occurrences = accountProjection.Dates.Where(d => d.ExpensesPaid > 0).ToList();
                occurrences.Should().HaveCount(6);

                for (int i = 0; i < expectedDates.Length; i++)
                {
                    occurrences[i].Date.Should().Be(expectedDates[i]);
                    occurrences[i].ExpensesPaid.Should().Be(1500.0d);
                    occurrences[i].ExpenseItems.Should().ContainSingle(e => e.Description == "Rent");
                }

                // Validate final balance
                var expectedFinalBalance = 20000.0d - (6 * 1500.0d); // 20000 - 9000 = 11000
                accountProjection.Dates[179].Balance.Should().Be(expectedFinalBalance);
            }

            [Fact]
            public async Task Should_Handle_Mixed_Frequency_Incomes_And_Expenses_For_Six_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 1000.0d);

                var biWeeklySalary = EntityFactory.CreateIncome(account, false, "Bi-Weekly Salary", 1500.0d, "2025-01-17", null, Frequency.Weeks, 2);
                var monthlyRent = EntityFactory.CreateExpense(account, false, "Rent", 1200.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var weeklyGroceries = EntityFactory.CreateExpense(account, false, "Groceries", 100.0d, "2025-01-01", "2025-01-20", null, Frequency.Weeks, 1);

                account.Incomes.Add(biWeeklySalary);
                account.Expenses.Add(monthlyRent);
                account.Expenses.Add(weeklyGroceries);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 180
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jul 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 180);

                // Verify bi-weekly salary occurrences (13 times in 180 days)
                var salaryDays = accountProjection.Dates.Where(d => d.IncomeReceived > 0).ToList();
                salaryDays.Should().HaveCountGreaterThanOrEqualTo(12);
                salaryDays.Should().HaveCountLessThanOrEqualTo(14);
                salaryDays.Should().AllSatisfy(d =>
                {
                    d.IncomeReceived.Should().Be(1500.0d);
                    d.IncomeItems.Should().ContainSingle(i => i.Description == "Bi-Weekly Salary");
                });

                // Verify monthly rent (6 times)
                var rentDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Rent")).ToList();
                rentDays.Should().HaveCount(6);
                rentDays.Should().AllSatisfy(d => d.ExpenseItems.Should().Contain(e => e.Description == "Rent" && e.Amount == 1200.0d));

                // Verify weekly groceries (approximately 25-26 times)
                var groceryDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Groceries")).ToList();
                groceryDays.Should().HaveCountGreaterThanOrEqualTo(23);
                groceryDays.Should().HaveCountLessThanOrEqualTo(27);
                groceryDays.Should().AllSatisfy(d => d.ExpenseItems.Should().Contain(e => e.Description == "Groceries" && e.Amount == 100.0d));

                // Verify final day exists and has valid balance
                accountProjection.Dates[179].Date.Should().Be(new DateOnly(2025, 7, 13));
            }
        }

        // ===== 9 MONTH PROJECTIONS =====
        public class NineMonthProjections : GetFinancialProjectionsAsync
        {
            [Fact]
            public async Task Should_Project_Quarterly_Expenses_For_Nine_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 20000.0d);
                var quarterly = EntityFactory.CreateExpense(account, false, "Quarterly Tax", 2000.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 3);
                account.Expenses.Add(quarterly);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 270
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Oct 11)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 270);

                // Days 0-15 (Jan 15 - Jan 30): Before first quarterly expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 20000.0d);

                // Day 16 (Jan 31): First quarterly expense
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 18000.0d,
                    expectedExpensesPaid: 2000.0d,
                    expectedExpenseDescriptions: ["Quarterly Tax"]);

                // Days 17-104 (Feb 1 - Apr 29): Between first and second quarterly expense
                ValidateNoActivityRange(accountProjection.Dates, 17, 104, expectedBalance: 18000.0d);

                // Day 105 (Apr 30): Second quarterly expense
                ValidateEventDay(
                    accountProjection.Dates[105],
                    expectedDate: new DateOnly(2025, 4, 30),
                    expectedBalance: 16000.0d,
                    expectedExpensesPaid: 2000.0d,
                    expectedExpenseDescriptions: ["Quarterly Tax"]);

                // Days 106-195 (May 1 - Jul 29): Between second and third quarterly expense
                ValidateNoActivityRange(accountProjection.Dates, 106, 195, expectedBalance: 16000.0d);

                // Day 196 (Jul 30): Third quarterly expense
                ValidateEventDay(
                    accountProjection.Dates[196],
                    expectedDate: new DateOnly(2025, 7, 30),
                    expectedBalance: 14000.0d,
                    expectedExpensesPaid: 2000.0d,
                    expectedExpenseDescriptions: ["Quarterly Tax"]);

                // Days 197-269 (Jul 31 - Oct 11): After third expense
                ValidateNoActivityRange(accountProjection.Dates, 197, 269, expectedBalance: 14000.0d);
            }

            [Fact]
            public async Task Should_Project_Expense_With_Multiple_Renewals_For_Nine_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 10000.0d);
                var biMonthly = EntityFactory.CreateExpense(account, false, "Bi-Monthly Subscription", 99.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 2);
                account.Expenses.Add(biMonthly);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 270
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Oct 11)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 270);

                // Days 0-15 (Jan 15 - Jan 30): Before first bi-monthly expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 10000.0d);

                // Day 16 (Jan 31): First bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 9901.0d,
                    expectedExpensesPaid: 99.0d,
                    expectedExpenseDescriptions: ["Bi-Monthly Subscription"]);

                // Days 17-74 (Feb 1 - Mar 30): Between first and second expense
                ValidateNoActivityRange(accountProjection.Dates, 17, 74, expectedBalance: 9901.0d);

                // Day 75 (Mar 31): Second bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[75],
                    expectedDate: new DateOnly(2025, 3, 31),
                    expectedBalance: 9802.0d,
                    expectedExpensesPaid: 99.0d,
                    expectedExpenseDescriptions: ["Bi-Monthly Subscription"]);

                // Days 76-135 (Apr 1 - May 30): Between second and third expense
                ValidateNoActivityRange(accountProjection.Dates, 76, 135, expectedBalance: 9802.0d);

                // Day 136 (May 31): Third bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[136],
                    expectedDate: new DateOnly(2025, 5, 31),
                    expectedBalance: 9703.0d,
                    expectedExpensesPaid: 99.0d,
                    expectedExpenseDescriptions: ["Bi-Monthly Subscription"]);

                // Days 137-196 (Jun 1 - Jul 30): Between third and fourth expense
                ValidateNoActivityRange(accountProjection.Dates, 137, 196, expectedBalance: 9703.0d);

                // Day 197 (Jul 31): Fourth bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[197],
                    expectedDate: new DateOnly(2025, 7, 31),
                    expectedBalance: 9604.0d,
                    expectedExpensesPaid: 99.0d,
                    expectedExpenseDescriptions: ["Bi-Monthly Subscription"]);

                // Days 198-257 (Aug 1 - Sep 29): Between fourth and fifth expense
                ValidateNoActivityRange(accountProjection.Dates, 198, 257, expectedBalance: 9604.0d);

                // Day 258 (Sep 30): Fifth bi-monthly expense
                ValidateEventDay(
                    accountProjection.Dates[258],
                    expectedDate: new DateOnly(2025, 9, 30),
                    expectedBalance: 9505.0d,
                    expectedExpensesPaid: 99.0d,
                    expectedExpenseDescriptions: ["Bi-Monthly Subscription"]);

                // Days 259-269 (Oct 1 - Oct 11): After fifth expense
                ValidateNoActivityRange(accountProjection.Dates, 259, 269, expectedBalance: 9505.0d);
            }

            [Fact]
            public async Task Should_Handle_Complex_Multi_Account_Scenario_For_Nine_Months()
            {
                var checking = EntityFactory.CreateAccount("Checking", 5000.0d);
                var savings = EntityFactory.CreateAccount("Savings", 20000.0d);
                var credit = EntityFactory.CreateAccount("Credit Card", -500.0d);

                var salary = EntityFactory.CreateIncome(checking, false, "Salary", 5000.0d, "2025-01-31", null, Frequency.Months, 1);
                var rent = EntityFactory.CreateExpense(checking, false, "Rent", 1500.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var utilities = EntityFactory.CreateExpense(checking, false, "Utilities", 200.0d, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);

                var savingsTransfer = EntityFactory.CreateExpense(checking, false, "Savings Transfer", 1000.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var ccPayment = EntityFactory.CreateExpense(checking, false, "CC Payment", 500.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);

                checking.Incomes.Add(salary);
                checking.Expenses.Add(rent);
                checking.Expenses.Add(utilities);
                checking.Expenses.Add(savingsTransfer);
                checking.Expenses.Add(ccPayment);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 270
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([checking, savings, credit]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                result.Value.Accounts.Should().HaveCount(3);

                var checkingProjection = result.Value.Accounts.First(a => a.Description == "Checking");
                var savingsProjection = result.Value.Accounts.First(a => a.Description == "Savings");
                var creditProjection = result.Value.Accounts.First(a => a.Description == "Credit Card");

                // Validate all accounts have 270 consecutive dates (Jan 15 - Oct 11)
                ValidateConsecutiveDates(checkingProjection.Dates, _currentDate, 270);
                ValidateConsecutiveDates(savingsProjection.Dates, _currentDate, 270);
                ValidateConsecutiveDates(creditProjection.Dates, _currentDate, 270);

                // Checking account validation:
                // Days 0-4 (Jan 15 - Jan 19): Before CC payment
                ValidateNoActivityRange(checkingProjection.Dates, 0, 4, expectedBalance: 5000.0d);

                // Day 5 (Jan 20): CC payment
                ValidateEventDay(
                    checkingProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 4500.0d,
                    expectedExpensesPaid: 500.0d,
                    expectedExpenseDescriptions: ["CC Payment"]);

                // Days 6-9 (Jan 21 - Jan 24): After CC payment, before utilities
                ValidateNoActivityRange(checkingProjection.Dates, 6, 9, expectedBalance: 4500.0d);

                // Day 10 (Jan 25): Utilities
                ValidateEventDay(
                    checkingProjection.Dates[10],
                    expectedDate: new DateOnly(2025, 1, 25),
                    expectedBalance: 4300.0d,
                    expectedExpensesPaid: 200.0d,
                    expectedExpenseDescriptions: ["Utilities"]);

                // Days 11-15 (Jan 26 - Jan 30): After utilities, before month-end transactions
                ValidateNoActivityRange(checkingProjection.Dates, 11, 15, expectedBalance: 4300.0d);

                // Day 16 (Jan 31): Salary, rent, and savings transfer
                ValidateEventDay(
                    checkingProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 6800.0d,
                    expectedIncomeReceived: 5000.0d,
                    expectedExpensesPaid: 2500.0d,
                    expectedIncomeDescriptions: ["Salary"],
                    expectedExpenseDescriptions: ["Rent", "Savings Transfer"]);

                // Validate 9 monthly cycles of: CC payment (20th) → utilities (25th) → salary/rent/transfer (28-31st)
                // Month 2 (Feb): Days 17-44
                var feb20 = checkingProjection.Dates[36]; // Jan 15 + 36 = Feb 20
                feb20.Date.Should().Be(new DateOnly(2025, 2, 20));
                feb20.ExpensesPaid.Should().Be(500.0d);

                var feb25 = checkingProjection.Dates[41]; // Jan 15 + 41 = Feb 25
                feb25.Date.Should().Be(new DateOnly(2025, 2, 25));
                feb25.ExpensesPaid.Should().Be(200.0d);

                var feb28 = checkingProjection.Dates[44]; // Jan 15 + 44 = Feb 28
                feb28.Date.Should().Be(new DateOnly(2025, 2, 28));
                feb28.IncomeReceived.Should().Be(5000.0d);
                feb28.ExpenseItems.Should().Contain(e => e.Description == "Rent");
                feb28.ExpenseItems.Should().Contain(e => e.Description == "Savings Transfer");

                // Savings and credit accounts should remain constant (no transactions)
                savingsProjection.Dates.Should().AllSatisfy(d =>
                {
                    d.Balance.Should().Be(20000.0d);
                    d.IncomeReceived.Should().Be(0.0d);
                    d.ExpensesPaid.Should().Be(0.0d);
                });

                creditProjection.Dates.Should().AllSatisfy(d =>
                {
                    d.Balance.Should().Be(-500.0d);
                    d.IncomeReceived.Should().Be(0.0d);
                    d.ExpensesPaid.Should().Be(0.0d);
                });
            }
        }

        // ===== 12 MONTH PROJECTIONS (FULL YEAR) =====
        public class TwelveMonthProjections : GetFinancialProjectionsAsync
        {
            [Fact]
            public async Task Should_Project_Annual_Expense_For_Twelve_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 30000.0d);
                var annual = EntityFactory.CreateExpense(account, false, "Annual Insurance", 1200.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 12);
                account.Expenses.Add(annual);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 365
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jan 14 next year)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 365);

                // Days 0-15 (Jan 15 - Jan 30): Before annual expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 15, expectedBalance: 30000.0d);

                // Day 16 (Jan 31): Annual expense
                ValidateEventDay(
                    accountProjection.Dates[16],
                    expectedDate: new DateOnly(2025, 1, 31),
                    expectedBalance: 28800.0d,
                    expectedExpensesPaid: 1200.0d,
                    expectedExpenseDescriptions: ["Annual Insurance"]);

                // Days 17-364 (Feb 1 - Jan 14 next year): After annual expense, no more occurrences
                ValidateNoActivityRange(accountProjection.Dates, 17, 364, expectedBalance: 28800.0d);
            }

            [Fact]
            public async Task Should_Handle_Leap_Year_Calculations_For_Twelve_Months()
            {
                // 2024 is a leap year, but 2025 is not
                var account = EntityFactory.CreateAccount("Checking", 50000.0d);
                var monthly = EntityFactory.CreateExpense(account, false, "Monthly Payment", 500.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                account.Expenses.Add(monthly);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 365
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jan 14 next year)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 365);

                // Verify monthly occurrences (12 times) with correct dates and amounts
                var occurrences = accountProjection.Dates.Where(d => d.ExpensesPaid > 0).ToList();
                occurrences.Should().HaveCount(12);

                // Verify all occurrences have correct amount and description
                occurrences.Should().AllSatisfy(o =>
                {
                    o.ExpensesPaid.Should().Be(500.0d);
                    o.ExpenseItems.Should().ContainSingle(e => e.Description == "Monthly Payment");
                });

                // Verify February has the correct last day (28 in 2025, not a leap year)
                var februaryOccurrence = occurrences.FirstOrDefault(d => d.Date.Month == 2);
                februaryOccurrence.Should().NotBeNull();
                februaryOccurrence!.Date.Should().Be(new DateOnly(2025, 2, 28));

                // Verify all monthly dates
                occurrences[0].Date.Should().Be(new DateOnly(2025, 1, 31));
                occurrences[1].Date.Should().Be(new DateOnly(2025, 2, 28)); // Non-leap year
                occurrences[2].Date.Should().Be(new DateOnly(2025, 3, 28));
                occurrences[3].Date.Should().Be(new DateOnly(2025, 4, 28));
                occurrences[4].Date.Should().Be(new DateOnly(2025, 5, 28));
                occurrences[5].Date.Should().Be(new DateOnly(2025, 6, 28));
                occurrences[6].Date.Should().Be(new DateOnly(2025, 7, 28));
                occurrences[7].Date.Should().Be(new DateOnly(2025, 8, 28));
                occurrences[8].Date.Should().Be(new DateOnly(2025, 9, 28));
                occurrences[9].Date.Should().Be(new DateOnly(2025, 10, 28));
                occurrences[10].Date.Should().Be(new DateOnly(2025, 11, 28));
                occurrences[11].Date.Should().Be(new DateOnly(2025, 12, 28));

                // Verify final balance after 12 monthly expenses
                accountProjection.Dates[364].Balance.Should().Be(44000.0d); // 50000 - (12 * 500)
            }

            [Fact]
            public async Task Should_Project_All_Twelve_Months_With_Various_Frequencies()
            {
                var account = EntityFactory.CreateAccount("Checking", 30000.0d);

                var monthlySalary = EntityFactory.CreateIncome(account, false, "Monthly Salary", 5000.0d, "2025-01-31", null, Frequency.Months, 1);
                var monthlyRent = EntityFactory.CreateExpense(account, false, "Rent", 1500.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var quarterlyTax = EntityFactory.CreateExpense(account, false, "Quarterly Tax", 1000.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 3);
                var semiAnnualInsurance = EntityFactory.CreateExpense(account, false, "Insurance", 600.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 6);
                var annualMembership = EntityFactory.CreateExpense(account, false, "Membership", 300.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 12);

                account.Incomes.Add(monthlySalary);
                account.Expenses.Add(monthlyRent);
                account.Expenses.Add(quarterlyTax);
                account.Expenses.Add(semiAnnualInsurance);
                account.Expenses.Add(annualMembership);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 365
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jan 14 next year)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 365);

                // Verify monthly salary (12 times)
                var salaryOccurrences = accountProjection.Dates.Where(d => d.IncomeReceived > 0).ToList();
                salaryOccurrences.Should().HaveCount(12);
                salaryOccurrences.Should().AllSatisfy(o =>
                {
                    o.IncomeReceived.Should().Be(5000.0d);
                    o.IncomeItems.Should().ContainSingle(i => i.Description == "Monthly Salary");
                });

                // Verify monthly rent (12 times)
                var rentOccurrences = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Rent")).ToList();
                rentOccurrences.Should().HaveCount(12);
                rentOccurrences.Should().AllSatisfy(o =>
                {
                    o.ExpenseItems.Should().Contain(e => e.Description == "Rent" && e.Amount == 1500.0d);
                });

                // Verify quarterly tax (4 times: Jan 31, Apr 30, Jul 30, Oct 30)
                var taxOccurrences = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Quarterly Tax")).ToList();
                taxOccurrences.Should().HaveCount(4);
                taxOccurrences[0].Date.Should().Be(new DateOnly(2025, 1, 31));
                taxOccurrences[1].Date.Should().Be(new DateOnly(2025, 4, 30));
                taxOccurrences[2].Date.Should().Be(new DateOnly(2025, 7, 30));
                taxOccurrences[3].Date.Should().Be(new DateOnly(2025, 10, 30));
                taxOccurrences.Should().AllSatisfy(o =>
                {
                    o.ExpenseItems.Should().Contain(e => e.Description == "Quarterly Tax" && e.Amount == 1000.0d);
                });

                // Verify semi-annual insurance (2 times: Jan 31, Jul 31)
                var insuranceOccurrences = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Insurance")).ToList();
                insuranceOccurrences.Should().HaveCount(2);
                insuranceOccurrences[0].Date.Should().Be(new DateOnly(2025, 1, 31));
                insuranceOccurrences[1].Date.Should().Be(new DateOnly(2025, 7, 31));
                insuranceOccurrences.Should().AllSatisfy(o =>
                {
                    o.ExpenseItems.Should().Contain(e => e.Description == "Insurance" && e.Amount == 600.0d);
                });

                // Verify annual membership (1 time: Jan 31)
                var membershipOccurrences = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Membership")).ToList();
                membershipOccurrences.Should().HaveCount(1);
                membershipOccurrences[0].Date.Should().Be(new DateOnly(2025, 1, 31));
                membershipOccurrences[0].ExpenseItems.Should().Contain(e => e.Description == "Membership" && e.Amount == 300.0d);

                // Verify final balance calculation
                // Starting: 30000
                // Income: 12 * 5000 = 60000
                // Expenses: (12 * 1500) + (4 * 1000) + (2 * 600) + (1 * 300) = 18000 + 4000 + 1200 + 300 = 23500
                // Final: 30000 + 60000 - 23500 = 66500
                accountProjection.Dates[364].Balance.Should().Be(66500.0d);
            }

            [Fact]
            public async Task Should_Handle_Daily_Expense_For_Twelve_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 50000.0d);
                var daily = EntityFactory.CreateExpense(account, false, "Daily Coffee", 5.0d, "2025-01-01", "2025-01-16", null, Frequency.Days, 1);
                account.Expenses.Add(daily);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 365
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jan 14 next year)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 365);

                // Day 0 (Jan 15): Before first daily expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 0, expectedBalance: 50000.0d);

                // Days 1-364 (Jan 16 - Jan 14 next year): Daily coffee every day (364 occurrences)
                var occurrences = accountProjection.Dates.Where(d => d.ExpensesPaid > 0).ToList();
                occurrences.Should().HaveCount(364);

                // Verify all occurrences have correct amount and description
                occurrences.Should().AllSatisfy(o =>
                {
                    o.ExpensesPaid.Should().Be(5.0d);
                    o.ExpenseItems.Should().ContainSingle(e => e.Description == "Daily Coffee");
                });

                // Verify continuous daily occurrence (consecutive dates)
                for (int i = 0; i < occurrences.Count - 1; i++)
                {
                    var dayDiff = occurrences[i + 1].Date.DayNumber - occurrences[i].Date.DayNumber;
                    dayDiff.Should().Be(1, $"day {i} ({occurrences[i].Date}) to day {i + 1} ({occurrences[i + 1].Date}) should be consecutive");
                }

                // Verify first and last occurrence dates
                occurrences[0].Date.Should().Be(new DateOnly(2025, 1, 16));
                occurrences[363].Date.Should().Be(new DateOnly(2026, 1, 14));

                // Verify final balance after 364 daily expenses
                accountProjection.Dates[364].Balance.Should().Be(48180.0d); // 50000 - (364 * 5)
            }

            [Fact]
            public async Task Should_Handle_Expense_Starting_In_Future_For_Twelve_Months()
            {
                var account = EntityFactory.CreateAccount("Checking", 20000.0d);
                var futureExpense = EntityFactory.CreateExpense(account, false, "Future Subscription", 50.0d, "2025-06-01", "2025-06-15", null, Frequency.Months, 1);
                account.Expenses.Add(futureExpense);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 365
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jan 14 next year)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 365);

                // Days 0-150 (Jan 15 - Jun 14): Before future expense starts, no activity
                ValidateNoActivityRange(accountProjection.Dates, 0, 150, expectedBalance: 20000.0d);

                // Day 151 (Jun 15): First monthly expense
                ValidateEventDay(
                    accountProjection.Dates[151],
                    expectedDate: new DateOnly(2025, 6, 15),
                    expectedBalance: 19950.0d,
                    expectedExpensesPaid: 50.0d,
                    expectedExpenseDescriptions: ["Future Subscription"]);

                // Verify all monthly occurrences starting from June 15
                var occurrences = accountProjection.Dates.Where(d => d.ExpensesPaid > 0).ToList();
                occurrences.Should().HaveCountGreaterThanOrEqualTo(7); // Jun, Jul, Aug, Sep, Oct, Nov, Dec, Jan

                occurrences[0].Date.Should().Be(new DateOnly(2025, 6, 15));
                occurrences.Should().AllSatisfy(o =>
                {
                    o.ExpensesPaid.Should().Be(50.0d);
                    o.ExpenseItems.Should().ContainSingle(e => e.Description == "Future Subscription");
                });

                // Verify final balance after all occurrences
                var expectedFinalBalance = 20000.0d - (occurrences.Count * 50.0d);
                accountProjection.Dates[364].Balance.Should().Be(expectedFinalBalance);
            }

            [Fact]
            public async Task Should_Project_Complex_Realistic_Scenario_For_Full_Year()
            {
                var checking = EntityFactory.CreateAccount("Checking", 2000.0d);

                // Income
                var biWeeklySalary = EntityFactory.CreateIncome(checking, false, "Salary", 2000.0d, "2025-01-17", null, Frequency.Weeks, 2);

                // Fixed monthly expenses
                var rent = EntityFactory.CreateExpense(checking, false, "Rent", 1200.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var utilities = EntityFactory.CreateExpense(checking, false, "Utilities", 150.0d, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                var internet = EntityFactory.CreateExpense(checking, false, "Internet", 60.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);

                // Variable expenses
                var weeklyGroceries = EntityFactory.CreateExpense(checking, false, "Groceries", 100.0d, "2025-01-01", "2025-01-20", null, Frequency.Weeks, 1);
                var biWeeklyGas = EntityFactory.CreateExpense(checking, false, "Gas", 60.0d, "2025-01-01", "2025-01-18", null, Frequency.Weeks, 2);

                // Periodic expenses
                var quarterlyInsurance = EntityFactory.CreateExpense(checking, false, "Insurance", 300.0d, "2025-01-01", "2025-01-31", null, Frequency.Months, 3);
                var annualSubscription = EntityFactory.CreateExpense(checking, false, "Prime", 139.0d, "2025-01-01", "2025-01-15", "2025-01-15", Frequency.OneTime, 1);

                checking.Incomes.Add(biWeeklySalary);
                checking.Expenses.Add(rent);
                checking.Expenses.Add(utilities);
                checking.Expenses.Add(internet);
                checking.Expenses.Add(weeklyGroceries);
                checking.Expenses.Add(biWeeklyGas);
                checking.Expenses.Add(quarterlyInsurance);
                checking.Expenses.Add(annualSubscription);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 365
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([checking]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Jan 14 next year)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 365);

                // Day 0 (Jan 15): Prime expense paid on start date
                ValidateEventDay(
                    accountProjection.Dates[0],
                    expectedDate: _currentDate,
                    expectedBalance: 1861.0d,
                    expectedExpensesPaid: 139.0d,
                    expectedExpenseDescriptions: ["Prime"]);

                // Verify bi-weekly salary (approximately 26 times in a year)
                var salaryDays = accountProjection.Dates.Where(d => d.IncomeReceived > 0).ToList();
                salaryDays.Should().HaveCountGreaterThanOrEqualTo(24);
                salaryDays.Should().HaveCountLessThanOrEqualTo(27);
                salaryDays.Should().AllSatisfy(d =>
                {
                    d.IncomeReceived.Should().Be(2000.0d);
                    d.IncomeItems.Should().ContainSingle(i => i.Description == "Salary");
                });

                // Verify monthly rent (12 times)
                var rentDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Rent")).ToList();
                rentDays.Should().HaveCount(12);
                rentDays.Should().AllSatisfy(d =>
                {
                    d.ExpenseItems.Should().Contain(e => e.Description == "Rent" && e.Amount == 1200.0d);
                });

                // Verify monthly utilities (12 times)
                var utilityDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Utilities")).ToList();
                utilityDays.Should().HaveCount(12);
                utilityDays.Should().AllSatisfy(d =>
                {
                    d.ExpenseItems.Should().Contain(e => e.Description == "Utilities" && e.Amount == 150.0d);
                });

                // Verify monthly internet (12 times)
                var internetDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Internet")).ToList();
                internetDays.Should().HaveCount(12);
                internetDays.Should().AllSatisfy(d =>
                {
                    d.ExpenseItems.Should().Contain(e => e.Description == "Internet" && e.Amount == 60.0d);
                });

                // Verify weekly groceries (approximately 52 times)
                var groceryDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Groceries")).ToList();
                groceryDays.Should().HaveCountGreaterThanOrEqualTo(50);
                groceryDays.Should().HaveCountLessThanOrEqualTo(54);
                groceryDays.Should().AllSatisfy(d =>
                {
                    d.ExpenseItems.Should().Contain(e => e.Description == "Groceries" && e.Amount == 100.0d);
                });

                // Verify bi-weekly gas (approximately 26 times)
                var gasDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Gas")).ToList();
                gasDays.Should().HaveCountGreaterThanOrEqualTo(24);
                gasDays.Should().HaveCountLessThanOrEqualTo(27);
                gasDays.Should().AllSatisfy(d =>
                {
                    d.ExpenseItems.Should().Contain(e => e.Description == "Gas" && e.Amount == 60.0d);
                });

                // Verify quarterly insurance (4 times)
                var insuranceDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Insurance")).ToList();
                insuranceDays.Should().HaveCount(4);
                insuranceDays.Should().AllSatisfy(d =>
                {
                    d.ExpenseItems.Should().Contain(e => e.Description == "Insurance" && e.Amount == 300.0d);
                });

                // Verify one-time Prime expense (1 time, already validated as first day)
                var primeDays = accountProjection.Dates.Where(d => d.ExpenseItems.Any(e => e.Description == "Prime")).ToList();
                primeDays.Should().HaveCount(1);
                primeDays[0].Date.Should().Be(new DateOnly(2025, 1, 15));

                // Verify ending balance calculation
                // Starting: 2000, Prime paid on day 0: 2000 - 139 = 1861
                // Income: bi-weekly salary (actual count from test)
                // Expenses: rent(12) + utilities(12) + internet(12) + groceries(~52) + gas(~26) + insurance(4) + prime(1)
                var finalDay = accountProjection.Dates.Last();
                finalDay.Date.Should().Be(new DateOnly(2026, 1, 14));

                // Calculate exact final balance based on actual occurrences
                var totalIncome = salaryDays.Count * 2000.0d;
                var totalExpenses = 139.0d + // Prime (day 0)
                                   (rentDays.Count * 1200.0d) +
                                   (utilityDays.Count * 150.0d) +
                                   (internetDays.Count * 60.0d) +
                                   (groceryDays.Count * 100.0d) +
                                   (gasDays.Count * 60.0d) +
                                   (insuranceDays.Count * 300.0d);
                var expectedFinalBalance = 2000.0d + totalIncome - totalExpenses;

                finalDay.Balance.Should().Be(expectedFinalBalance,
                    $"Final balance = starting(2000) + income({totalIncome}) - expenses({totalExpenses}) = {expectedFinalBalance}");
            }
        }

        // ===== EDGE CASES AND SPECIAL SCENARIOS =====
        public class EdgeCasesAndSpecialScenarios : GetFinancialProjectionsAsync
        {
            [Fact]
            public async Task Should_Handle_Account_With_Reserved_Funds()
            {
                var account = EntityFactory.CreateAccount("Checking", 5000.0d, reserved: 1000.0d);
                var expense = EntityFactory.CreateExpense(account, false, "Bill", 200.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                account.Expenses.Add(expense);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-4 (Jan 15 - Jan 19): Before expense payment
                // Expense accrues from Jan 1 to Jan 20 (19 days), daily rate = 200/19 = 10.526315789...
                // Available = Balance - Reserved - Accrued + ExpensesPaid (ExpensesPaid = 0 on non-payment days)
                var billDailyAccrual = 200.0d / 19.0d;

                for (int i = 0; i <= 4; i++)
                {
                    var dayNumber = i + 1;
                    var daysAccrued = 14 + i; // Jan 1 to Jan 15 = 14 days, then 15, 16, 17, 18
                    var accruedAmount = Math.Round(billDailyAccrual * daysAccrued, 2, MidpointRounding.AwayFromZero);
                    var expectedAvailable = 5000.0d - 1000.0d - accruedAmount + 0.0d; // +0 for expensesPaid

                    accountProjection.Dates[i].Balance.Should().Be(5000.0d);
                    accountProjection.Dates[i].Available.Should().Be(expectedAvailable,
                        $"day {dayNumber}: available = balance(5000) - reserved(1000) - accrued({accruedAmount:F2}) + expensesPaid(0) = {expectedAvailable:F2}");
                }

                // Day 5 (Jan 20): Expense paid
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 4800.0d,
                    expectedExpensesPaid: 200.0d,
                    expectedExpenseDescriptions: ["Bill"]);

                // After payment, expense renews for next period (Feb 20, 31 days away)
                // New daily accrual = 200 / 31 = 6.451612903...
                // Available on payment day = Balance - Reserved - Accrued + ExpensesPaid
                // Available = 4800 - 1000 - 0 + 200 = 4000
                var nextBillDailyAccrual = 200.0d / 31.0d;
                accountProjection.Dates[5].Available.Should().Be(4000.0d,
                    "day 6: available = balance(4800) - reserved(1000) - accrued(0, just reset) + expensesPaid(200) = 4000");

                // Days 6-29 (Jan 21 - Feb 13): After expense, balance constant at 4800
                // Accrual accumulates for next period (due Feb 20)
                // Available = Balance - Reserved - Accrued + ExpensesPaid (ExpensesPaid = 0 on non-payment days)
                for (int i = 6; i <= 29; i++)
                {
                    var dayNumber = i + 1;
                    var daysIntoNextPeriod = i - 5; // Days since Jan 20 payment
                    var accruedForNextPeriod = Math.Round(nextBillDailyAccrual * daysIntoNextPeriod, 2, MidpointRounding.AwayFromZero);
                    var expectedAvailable = 4800.0d - 1000.0d - accruedForNextPeriod + 0.0d; // +0 for expensesPaid

                    accountProjection.Dates[i].Balance.Should().Be(4800.0d);
                    accountProjection.Dates[i].Available.Should().Be(expectedAvailable,
                        $"day {dayNumber}: available = balance(4800) - reserved(1000) - accrued({accruedForNextPeriod:F2}) + expensesPaid(0) = {expectedAvailable:F2}");
                }
            }

            [Fact]
            public async Task Should_Handle_Negative_Account_Balance()
            {
                var account = EntityFactory.CreateAccount("Credit Card", -500.0d);
                var payment = EntityFactory.CreateIncome(account, false, "Payment", 600.0d, "2025-01-20", null, Frequency.Months, 1);
                account.Incomes.Add(payment);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-4 (Jan 15 - Jan 19): Before payment, negative balance
                ValidateNoActivityRange(accountProjection.Dates, 0, 4, expectedBalance: -500.0d);

                // Day 5 (Jan 20): Payment received, balance becomes positive
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 100.0d,
                    expectedIncomeReceived: 600.0d,
                    expectedIncomeDescriptions: ["Payment"]);

                // Days 6-29 (Jan 21 - Feb 13): After payment, positive balance
                ValidateNoActivityRange(accountProjection.Dates, 6, 29, expectedBalance: 100.0d);
            }

            [Fact]
            public async Task Should_Handle_Zero_Amount_Expense()
            {
                var account = EntityFactory.CreateAccount("Checking", 1000.0d);
                var zeroExpense = EntityFactory.CreateExpense(account, false, "Zero Expense", 0.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                account.Expenses.Add(zeroExpense);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // All days: Balance should remain unchanged at 1000 (zero-amount expense has no effect)
                ValidateNoActivityRange(accountProjection.Dates, 0, 29, expectedBalance: 1000.0d);
            }

            [Fact]
            public async Task Should_Handle_StartDate_In_Future()
            {
                var futureDate = _currentDate.AddDays(10);
                var account = EntityFactory.CreateAccount("Checking", 1000.0d);
                var expense = EntityFactory.CreateExpense(account, false, "Bill", 100.0d, "2025-01-01", "2025-01-30", null, Frequency.Months, 1);
                account.Expenses.Add(expense);

                var options = new ProjectionOptions
                {
                    StartDate = futureDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive starting from future date (Jan 25 - Feb 23)
                ValidateConsecutiveDates(accountProjection.Dates, futureDate, 30);

                // Days 0-4 (Jan 25 - Jan 29): Before expense
                ValidateNoActivityRange(accountProjection.Dates, 0, 4, expectedBalance: 1000.0d);

                // Day 5 (Jan 30): Expense paid
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 30),
                    expectedBalance: 900.0d,
                    expectedExpensesPaid: 100.0d,
                    expectedExpenseDescriptions: ["Bill"]);

                // Days 6-29 (Jan 31 - Feb 23): After expense
                ValidateNoActivityRange(accountProjection.Dates, 6, 29, expectedBalance: 900.0d);
            }

            [Fact]
            public async Task Should_Aggregate_Global_Projections_Correctly()
            {
                var account1 = EntityFactory.CreateAccount("Account 1", 1000.0d);
                var account2 = EntityFactory.CreateAccount("Account 2", 2000.0d);

                var income1 = EntityFactory.CreateIncome(account1, false, "Income 1", 500.0d, "2025-01-20", null, Frequency.Months, 1);
                var expense1 = EntityFactory.CreateExpense(account1, false, "Expense 1", 100.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);

                var income2 = EntityFactory.CreateIncome(account2, false, "Income 2", 300.0d, "2025-01-25", null, Frequency.Months, 1);
                var expense2 = EntityFactory.CreateExpense(account2, false, "Expense 2", 200.0d, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);

                account1.Incomes.Add(income1);
                account1.Expenses.Add(expense1);
                account2.Incomes.Add(income2);
                account2.Expenses.Add(expense2);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account1, account2]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var global = result.Value.Global;
                var account1Projection = result.Value.Accounts[0];
                var account2Projection = result.Value.Accounts[1];

                // Validate all accounts and global have 30 consecutive dates (Jan 15 - Feb 13)
                ValidateConsecutiveDates(account1Projection.Dates, _currentDate, 30);
                ValidateConsecutiveDates(account2Projection.Dates, _currentDate, 30);
                ValidateConsecutiveDates(global, _currentDate, 30);

                // Day 0 (Jan 15): Combined starting balances
                global[0].Balance.Should().Be(3000.0d); // 1000 + 2000

                // Days 1-4 (Jan 16 - Jan 19): Before first transaction
                ValidateNoActivityRange(global, 1, 4, expectedBalance: 3000.0d);

                // Day 5 (Jan 20): Income1 and expense1
                ValidateEventDay(
                    global[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 3400.0d, // 3000 + 500 - 100
                    expectedIncomeReceived: 500.0d,
                    expectedExpensesPaid: 100.0d);

                // Days 6-9 (Jan 21 - Jan 24): After first transaction, before second
                ValidateNoActivityRange(global, 6, 9, expectedBalance: 3400.0d);

                // Day 10 (Jan 25): Income2 and expense2
                ValidateEventDay(
                    global[10],
                    expectedDate: new DateOnly(2025, 1, 25),
                    expectedBalance: 3500.0d, // 3400 + 300 - 200
                    expectedIncomeReceived: 300.0d,
                    expectedExpensesPaid: 200.0d);

                // Days 11-29 (Jan 26 - Feb 13): After all transactions
                ValidateNoActivityRange(global, 11, 29, expectedBalance: 3500.0d);
            }

            [Fact]
            public async Task Should_Handle_Multiple_Expenses_On_Same_Day()
            {
                var account = EntityFactory.CreateAccount("Checking", 2000.0d);
                var expense1 = EntityFactory.CreateExpense(account, false, "Bill 1", 100.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                var expense2 = EntityFactory.CreateExpense(account, false, "Bill 2", 150.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                var expense3 = EntityFactory.CreateExpense(account, false, "Bill 3", 75.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                account.Expenses.Add(expense1);
                account.Expenses.Add(expense2);
                account.Expenses.Add(expense3);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Days 0-4 (Jan 15 - Jan 19): Before all expenses
                ValidateNoActivityRange(accountProjection.Dates, 0, 4, expectedBalance: 2000.0d);

                // Day 5 (Jan 20): All three expenses on same day
                ValidateEventDay(
                    accountProjection.Dates[5],
                    expectedDate: new DateOnly(2025, 1, 20),
                    expectedBalance: 1675.0d,
                    expectedExpensesPaid: 325.0d,
                    expectedExpenseDescriptions: ["Bill 1", "Bill 2", "Bill 3"]);

                // Verify all three expense items are present with correct amounts
                accountProjection.Dates[5].ExpenseItems.Should().HaveCount(3);
                accountProjection.Dates[5].ExpenseItems.Should().Contain(e => e.Description == "Bill 1" && e.Amount == 100.0d);
                accountProjection.Dates[5].ExpenseItems.Should().Contain(e => e.Description == "Bill 2" && e.Amount == 150.0d);
                accountProjection.Dates[5].ExpenseItems.Should().Contain(e => e.Description == "Bill 3" && e.Amount == 75.0d);

                // Days 6-29 (Jan 21 - Feb 13): After all expenses
                ValidateNoActivityRange(accountProjection.Dates, 6, 29, expectedBalance: 1675.0d);
            }

            [Fact]
            public async Task Should_Handle_Expense_Accruals_With_Different_Start_Dates()
            {
                var account = EntityFactory.CreateAccount("Checking", 5000.0d);

                // Expense 1: Accrual starts before projection start (Jan 10), due Jan 30
                // This should accrue from day 1 of projection (Jan 15)
                var expense1 = EntityFactory.CreateExpense(account, false, "Rent", 900.0d, "2025-01-10", "2025-01-30", null, Frequency.Months, 1);

                // Expense 2: Accrual starts on last day of projection (Feb 13), due Feb 20
                // This should never accrue during the 30-day forecast period
                var expense2 = EntityFactory.CreateExpense(account, false, "Insurance", 600.0d, "2025-02-13", "2025-02-20", null, Frequency.Months, 1);

                account.Expenses.Add(expense1);
                account.Expenses.Add(expense2);

                var options = new ProjectionOptions
                {
                    StartDate = _currentDate,
                    DaysForecast = 30
                };

                _repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(Arg.Any<CancellationToken>())
                    .Returns([account]);

                var result = await _service.GetFinancialProjectionsAsync(options, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();

                var accountProjection = result.Value.Accounts[0];

                // Validate all dates are consecutive (Jan 15 - Feb 13)
                ValidateConsecutiveDates(accountProjection.Dates, _currentDate, 30);

                // Calculate daily accrual for Rent: 900 / 20 days (Jan 10 to Jan 30) = 45 per day
                // Projection starts Jan 15, so 5 days already accrued (Jan 10-15 exclusive of Jan 10, inclusive of Jan 15)
                // DaysFromAccrualStart counts from AccrualStart to current date
                var rentDailyAccrual = 900.0d / 20.0d; // 45.0
                var rentDaysAccruedOnStart = 5; // Jan 10 to Jan 15 = 5 days

                // Days 0-14 (Jan 15 - Jan 29): Rent accruing daily, no payments yet
                for (int i = 0; i <= 14; i++)
                {
                    var projection = accountProjection.Dates[i];
                    var dayNumber = i + 1;
                    var daysAccrued = rentDaysAccruedOnStart + i; // Start with 5, then 6, 7, 8...
                    var totalAccrued = daysAccrued * rentDailyAccrual;
                    var expectedAvailable = 5000.0d - totalAccrued;

                    projection.Date.Should().Be(_currentDate.AddDays(i));
                    projection.Balance.Should().Be(5000.0d, $"day {dayNumber} balance should remain 5000");
                    projection.DailyAccrual.Should().Be(rentDailyAccrual, $"day {dayNumber} should have daily accrual of {rentDailyAccrual}");
                    projection.Available.Should().BeApproximately(expectedAvailable, 0.01, $"day {dayNumber} available should be {expectedAvailable} (5000 - {totalAccrued} accrued for {daysAccrued} days)");
                    projection.IncomeReceived.Should().Be(0.0d);
                    projection.ExpensesPaid.Should().Be(0.0d);
                }

                // Day 15 (Jan 30): Rent paid (900), expense renews for next period
                ValidateEventDay(
                    accountProjection.Dates[15],
                    expectedDate: new DateOnly(2025, 1, 30),
                    expectedBalance: 4100.0d,
                    expectedExpensesPaid: 900.0d,
                    expectedExpenseDescriptions: ["Rent"]);

                // After payment, expense renews and daily accrual starts for next period (Feb 28)
                // Next period: Jan 30 to Feb 28 = 29 days
                // On payment day, DailyExpenseAccrual = 900 / 29 = 31.03448275862069
                var nextPeriodDays = 29;
                var paymentDayDailyAccrual = 900.0d / nextPeriodDays;
                accountProjection.Dates[15].DailyAccrual.Should().BeApproximately(paymentDayDailyAccrual, 0.01, "daily accrual on payment day");

                // On payment day: Available = Balance - Reserved - Accrued + ExpensesPaid
                // Available = 4100 - 0 - 0 + 900 = 5000 (accrued reset to 0, expensesPaid added back)
                accountProjection.Dates[15].Available.Should().Be(5000.0d,
                    "available on payment day = balance(4100) - reserved(0) - accrued(0, just reset) + expensesPaid(900) = 5000");

                // Days 16-28 (Jan 31 - Feb 12): Rent now accruing for next period (due Feb 28)
                // DailyAccrual varies each day because it recalculates as: (remaining balance) / (days until due)
                // Available = Balance - accumulated accrual
                var baseAccrualRate = 900.0d / nextPeriodDays; // 31.03448275862069

                for (int i = 16; i <= 28; i++)
                {
                    var projection = accountProjection.Dates[i];
                    var dayNumber = i + 1;
                    var daysAccruedForNextPeriod = i - 15; // Days since Jan 30 (day 16 = 1, day 17 = 2, etc.)

                    // Calculate accrued amount based on base rate
                    var accumulatedAccrual = Math.Round(baseAccrualRate * daysAccruedForNextPeriod, 2, MidpointRounding.AwayFromZero);

                    // Remaining balance for the expense
                    var remainingBalance = 900.0d - accumulatedAccrual;

                    // Days remaining until Feb 28 from current date
                    var daysRemaining = nextPeriodDays - daysAccruedForNextPeriod;

                    // DailyBalance = remaining balance / days remaining
                    var expectedDailyAccrual = remainingBalance / daysRemaining;

                    // Available = account balance - accumulated accrual
                    var expectedAvailable = 4100.0d - accumulatedAccrual;

                    projection.Date.Should().Be(_currentDate.AddDays(i));
                    projection.Balance.Should().Be(4100.0d, $"day {dayNumber} balance should remain 4100");
                    projection.DailyAccrual.Should().BeApproximately(expectedDailyAccrual, 0.01, $"day {dayNumber} daily accrual should be {expectedDailyAccrual:F4}");
                    projection.Available.Should().Be(expectedAvailable, $"day {dayNumber} available should be {expectedAvailable:F2} (4100 - {accumulatedAccrual:F2} accrued)");
                    projection.IncomeReceived.Should().Be(0.0d);
                    projection.ExpensesPaid.Should().Be(0.0d);
                }

                // Day 29 (Feb 13): Last day - 14 days accrued for next rent period
                var lastDay = accountProjection.Dates[29];
                var lastDayAccrued = Math.Round(baseAccrualRate * 14, 2, MidpointRounding.AwayFromZero);
                var lastDayExpectedAvailable = 4100.0d - lastDayAccrued;

                lastDay.Date.Should().Be(new DateOnly(2025, 2, 13));
                lastDay.Balance.Should().Be(4100.0d);
                lastDay.Available.Should().Be(lastDayExpectedAvailable, $"available should be 4100 - {lastDayAccrued:F2} accrued");
                lastDay.IncomeReceived.Should().Be(0.0d);
                lastDay.ExpensesPaid.Should().Be(0.0d);
            }
        }
    }
}
