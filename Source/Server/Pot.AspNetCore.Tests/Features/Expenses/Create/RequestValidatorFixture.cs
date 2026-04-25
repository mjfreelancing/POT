using FluentValidation.Results;
using Pot.AspNetCore.Features.Expenses.Create;
using Pot.Shared.Enumerations;
using Shouldly;

namespace Pot.AspNetCore.Tests.Features.Expenses.Create;

public class RequestValidatorFixture
{
    [Fact]
    public void Should_Be_Valid_For_A_Valid_Request()
    {
        var request = CreateRequest();

        var validationResult = Validate(request);

        validationResult.IsValid.ShouldBeTrue();
        validationResult.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Return_Error_When_Description_Is_Empty()
    {
        var request = CreateRequest(description: string.Empty);

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.Description));
    }

    [Fact]
    public void Should_Return_Error_When_AccrualStart_Is_Set_And_AccrualPolicy_Is_None()
    {
        var request = CreateRequest(
            accrualPolicy: AccrualPolicy.None,
            accrualStart: new DateOnly(2026, 1, 5));

        var validationResult = Validate(request);

        ShouldContainError(
            validationResult,
            nameof(Request.AccrualStart),
            $"Must be empty when Accrual Policy is {AccrualPolicy.None.Name}");
    }

    [Fact]
    public void Should_Not_Return_AccrualStart_Error_When_AccrualStart_Is_Empty_And_AccrualPolicy_Is_None()
    {
        var request = CreateRequest(
            accrualPolicy: AccrualPolicy.None,
            accrualStart: null);

        var validationResult = Validate(request);

        ShouldNotContainError(validationResult, nameof(Request.AccrualStart));
    }

    [Fact]
    public void Should_Return_Error_When_AccrualStart_Is_After_EndDate()
    {
        var request = CreateRequest(
            accrualStart: new DateOnly(2026, 3, 1),
            endDate: new DateOnly(2026, 2, 1));

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.AccrualStart), "Cannot be after the end date");
    }

    [Fact]
    public void Should_Return_Error_When_EndDate_Is_Earlier_Than_NextDue()
    {
        var request = CreateRequest(
            nextDue: new DateOnly(2026, 3, 1),
            endDate: new DateOnly(2026, 2, 1));

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.EndDate), "Cannot be earlier than the next due date");
    }

    [Fact]
    public void Should_Return_Error_When_EndDate_Is_Set_For_OneTime_Frequency()
    {
        var request = CreateRequest(
            frequency: Frequency.OneTime,
            frequencyCount: 0,
            endDate: new DateOnly(2026, 2, 1),
            accrualStart: null);

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.EndDate), "A one-time expense does not have an end date");
    }

    [Fact]
    public void Should_Not_Return_EndDate_Error_When_EndDate_Is_Not_Set_For_OneTime_Frequency()
    {
        var request = CreateRequest(
            frequency: Frequency.OneTime,
            frequencyCount: 0,
            endDate: null,
            accrualStart: null);

        var validationResult = Validate(request);

        ShouldNotContainError(validationResult, nameof(Request.EndDate), "A one-time expense does not have an end date");
    }

    [Fact]
    public void Should_Return_Error_When_FrequencyCount_Is_Not_Zero_For_OneTime_Frequency()
    {
        var request = CreateRequest(
            frequency: Frequency.OneTime,
            frequencyCount: 1,
            accrualStart: null);

        var validationResult = Validate(request);

        ShouldContainError(
            validationResult,
            nameof(Request.FrequencyCount),
            $"Must be zero when Frequency is {Frequency.OneTime.Name}");
    }

    [Fact]
    public void Should_Return_Error_When_FrequencyCount_Is_Less_Than_One_For_Recurring_Frequency()
    {
        var request = CreateRequest(
            frequency: Frequency.Months,
            frequencyCount: 0);

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.FrequencyCount), "Must be greater than zero");
    }

    [Fact]
    public void Should_Return_Error_When_Amount_Is_Negative()
    {
        var request = CreateRequest(amount: -1);

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.Amount));
    }

    [Fact]
    public void Should_Return_Error_When_AccountRowId_Is_Empty()
    {
        var request = CreateRequest(accountRowId: Guid.Empty);

        var validationResult = Validate(request);

        ShouldContainError(validationResult, nameof(Request.AccountRowId));
    }

    private static ValidationResult Validate(Request request)
    {
        var validator = new RequestValidator();

        return validator.Validate(request, CreateValidationContext(request));
    }

    private static Request CreateRequest(
        string description = "Expense",
        DateOnly? accrualStart = null,
        DateOnly? nextDue = null,
        DateOnly? endDate = null,
        AccrualPolicy? accrualPolicy = null,
        Frequency? frequency = null,
        int frequencyCount = 1,
        double amount = 100,
        Guid? accountRowId = null)
    {
        var resolvedNextDue = nextDue ?? new DateOnly(2026, 2, 1);

        return new Request
        {
            Description = description,
            AccrualStart = accrualStart,
            NextDue = resolvedNextDue,
            EndDate = endDate,
            AccrualPolicy = accrualPolicy ?? AccrualPolicy.Automatic,
            Frequency = frequency ?? Frequency.Months,
            FrequencyCount = frequencyCount,
            Amount = amount,
            AccountRowId = accountRowId ?? Guid.NewGuid(),
            Note = null
        };
    }

    private static RequestValidationContext CreateValidationContext(Request request)
    {
        return new RequestValidationContext
        {
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            AccrualPolicy = request.AccrualPolicy,
            Frequency = request.Frequency
        };
    }

    private static void ShouldContainError(
        ValidationResult validationResult,
        string propertyName,
        string? message = null)
    {
        if (message is null)
        {
            validationResult.Errors.ShouldContain(error => error.PropertyName == propertyName);
            return;
        }

        validationResult.Errors.ShouldContain(error =>
            error.PropertyName == propertyName &&
            error.ErrorMessage == message);
    }

    private static void ShouldNotContainError(
        ValidationResult validationResult,
        string propertyName,
        string? message = null)
    {
        if (message is null)
        {
            validationResult.Errors.ShouldNotContain(error => error.PropertyName == propertyName);
            return;
        }

        validationResult.Errors.ShouldNotContain(error =>
            error.PropertyName == propertyName &&
            error.ErrorMessage == message);
    }
}
