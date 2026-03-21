using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Shouldly;

namespace Pot.TestUtils.Logging;

/// <summary>
/// Provides assertion helpers for <see cref="FakeLogCollector"/> and <see cref="FakeLogRecord"/> when validating
/// structured logging behavior in POT tests.
/// </summary>
/// <remarks>
/// These helpers are designed for contract-style test assertions around message text, category, log level, and
/// structured state values produced by APIs such as <c>LogCall(...)</c>.
/// </remarks>
public static class FakeLogCollectorExtensions
{
    /// <summary>
    /// Asserts that at least one collected log record satisfies the supplied predicate.
    /// </summary>
    /// <param name="collector">The fake log collector containing captured records.</param>
    /// <param name="predicate">A predicate that identifies the expected record.</param>
    /// <param name="customMessage">An optional assertion message used when no matching record is found.</param>
    /// <returns>The first matching <see cref="FakeLogRecord"/>.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Health check completed");
    ///
    /// // Fixture assertion code
    /// _ = collector.ShouldContainLog(record => record.Message == "Health check completed");
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainLog(this FakeLogCollector collector, Func<FakeLogRecord, bool> predicate,
        string? customMessage = null)
    {
        _ = collector.WhenNotNull();
        _ = predicate.WhenNotNull();

        var records = collector.GetSnapshot();
        var found = records.FirstOrDefault(record => predicate(record));

        found.ShouldNotBeNull(customMessage);

        return found;
    }

    /// <summary>
    /// Asserts that at least one collected record exists for the specified logger category.
    /// </summary>
    /// <param name="collector">The fake log collector containing captured records.</param>
    /// <param name="category">The expected logger category name.</param>
    /// <param name="customMessage">An optional assertion message used when no matching record is found.</param>
    /// <returns>The first matching <see cref="FakeLogRecord"/>.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Signed out");
    ///
    /// // Fixture assertion code
    /// _ = collector.ShouldContainLogInCategory("Pot.AspNetCore.Features.Auth.Logout.Handler");
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainLogInCategory(this FakeLogCollector collector, string category,
        string? customMessage = null)
    {
        _ = collector.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        var message = customMessage ?? $"Expected at least one log record in category '{category}'.";

        return collector.ShouldContainLog(record => string.Equals(record.Category, category, StringComparison.Ordinal), message);
    }

    /// <summary>
    /// Asserts that a log record exists with an exact category and message, and that expected structured-state values are present.
    /// </summary>
    /// <param name="collector">The fake log collector containing captured records.</param>
    /// <param name="category">The expected logger category name.</param>
    /// <param name="message">The expected rendered log message.</param>
    /// <param name="expectedStructuredState">The expected structured logging keys and values.</param>
    /// <param name="assertNoAdditionalStructuredState">
    /// When <see langword="true"/>, asserts that no additional structured-state keys exist beyond
    /// <paramref name="expectedStructuredState"/>.
    /// </param>
    /// <param name="customMessage">An optional assertion message used when no matching record is found.</param>
    /// <returns>The first matching <see cref="FakeLogRecord"/>.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Call: Invoke");
    ///
    /// // Fixture assertion code
    /// _ = collector.ShouldContainLogWithMessageAndStructuredState(
    ///     "Pot.AspNetCore.Features.Auth.Logout.Handler",
    ///     "Call: Invoke",
    ///     new Dictionary<string, object?>
    ///     {
    ///         ["MethodName"] = "Invoke",
    ///         ["{OriginalFormat}"] = "Call: {MethodName}"
    ///     });
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainLogWithMessageAndStructuredState(this FakeLogCollector collector,
        string category, string message, IReadOnlyDictionary<string, object?> expectedStructuredState,
        bool assertNoAdditionalStructuredState = false, string? customMessage = null)
    {
        _ = collector.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _ = expectedStructuredState.WhenNotNull();

        var record = collector.ShouldContainLog(record =>
            string.Equals(record.Category, category, StringComparison.Ordinal)
            && string.Equals(record.Message, message, StringComparison.Ordinal),
            customMessage ?? $"Expected a log in category '{category}' with message '{message}'.");

        record.ShouldHaveStructuredState(expectedStructuredState, assertNoAdditionalStructuredState);

        return record;
    }

    /// <summary>
    /// Asserts expected structured-state keys and values for a log record.
    /// </summary>
    /// <param name="record">The log record to validate.</param>
    /// <param name="expectedStructuredState">The expected structured logging keys and values.</param>
    /// <param name="assertNoAdditionalStructuredState">
    /// When <see langword="true"/>, asserts that no additional structured-state keys exist beyond
    /// <paramref name="expectedStructuredState"/>.
    /// </param>
    /// <returns>The same <paramref name="record"/> to support fluent assertions.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Call: Invoke");
    ///
    /// // Fixture setup + assertion code
    /// var record = collector.ShouldContainLog(r => r.Message == "Call: Invoke");
    /// _ = record.ShouldHaveStructuredState(new Dictionary<string, object?>
    /// {
    ///     ["MethodName"] = "Invoke",
    ///     ["{OriginalFormat}"] = "Call: {MethodName}"
    /// });
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldHaveStructuredState(this FakeLogRecord record,
        IReadOnlyDictionary<string, object?> expectedStructuredState, bool assertNoAdditionalStructuredState = false)
    {
        _ = record.WhenNotNull();
        _ = expectedStructuredState.WhenNotNull();

        foreach (var expectedEntry in expectedStructuredState)
        {
            var actualValue = record.GetStructuredStateValue(expectedEntry.Key);

            actualValue.ShouldNotBeNull($"Expected structured logging key '{expectedEntry.Key}' to be present.");
            actualValue.ShouldBe(expectedEntry.Value,
                $"Structured logging key '{expectedEntry.Key}' did not match the expected value.");
        }

        if (!assertNoAdditionalStructuredState)
        {
            return record;
        }

        var actualKeys = record.StructuredState!
            .Select(entry => entry.Key)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        actualKeys.ShouldBe(expectedStructuredState.Keys, ignoreOrder: true,
            customMessage: "Structured logging keys did not match the expected set.");

        return record;
    }

    /// <summary>
    /// Asserts a <c>LogCall(...)</c>-style record with expected category, method name, message template, and log level.
    /// </summary>
    /// <param name="collector">The fake log collector containing captured records.</param>
    /// <param name="category">The expected logger category name.</param>
    /// <param name="callerName">The expected caller/member name.</param>
    /// <param name="callerType">
    /// Optional caller type used to build method name values for instance-call scenarios. Use <see langword="null"/>
    /// for static-caller scenarios.
    /// </param>
    /// <param name="includeCallerNamespace">Whether the caller type name should include its namespace.</param>
    /// <param name="expectedLevel">The expected log level.</param>
    /// <param name="assertNoAdditionalStructuredState">
    /// When <see langword="true"/>, asserts that only expected <c>LogCall</c> state keys are present.
    /// </param>
    /// <param name="callPrefix">The call prefix used in the rendered message, such as <c>Call: </c>.</param>
    /// <param name="methodNameProperty">The structured-state key used for method name value.</param>
    /// <param name="originalFormatProperty">The structured-state key used for message template value.</param>
    /// <param name="customMessage">An optional assertion message used when no matching record is found.</param>
    /// <returns>The first matching <see cref="FakeLogRecord"/>.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogCall(null, callerName: "Invoke");
    ///
    /// // Fixture assertion code
    /// _ = collector.ShouldContainLogCall(
    ///     category: "Pot.AspNetCore.Features.Auth.Logout.Handler",
    ///     callerName: "Invoke",
    ///     callerType: null,
    ///     assertNoAdditionalStructuredState: false);
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainLogCall(this FakeLogCollector collector, string category,
        string callerName, Type? callerType = null, bool includeCallerNamespace = true,
        LogLevel expectedLevel = LogLevel.Information, bool assertNoAdditionalStructuredState = true,
        string callPrefix = "Call: ", string methodNameProperty = "MethodName",
        string originalFormatProperty = "{OriginalFormat}", string? customMessage = null)
    {
        _ = collector.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(callerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(callPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodNameProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFormatProperty);

        var methodNameValue = GetExpectedLogCallMethodName(callerName, callerType, includeCallerNamespace);
        var message = $"{callPrefix}{methodNameValue}";
        var template = GetLogCallTemplate(callPrefix, methodNameProperty);

        var record = collector.ShouldContainLogWithMessageAndStructuredState(
            category,
            message,
            new Dictionary<string, object?>
            {
                [methodNameProperty] = methodNameValue,
                [originalFormatProperty] = template
            },
            assertNoAdditionalStructuredState,
            customMessage);

        record.Level.ShouldBe(expectedLevel, "LogCall level did not match the expected level.");

        return record;
    }

    /// <summary>
    /// Asserts that the record contains a structured-state key.
    /// </summary>
    /// <param name="record">The log record to validate.</param>
    /// <param name="key">The expected structured-state key.</param>
    /// <returns>The same <paramref name="record"/> to support fluent assertions.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Call: Invoke");
    ///
    /// // Fixture setup + assertion code
    /// var record = collector.ShouldContainLog(r => r.Message == "Call: Invoke");
    /// _ = record.ShouldContainStructuredStateKey("MethodName");
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainStructuredStateKey(this FakeLogRecord record, string key)
    {
        _ = record.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var actualValue = record.GetStructuredStateValue(key);

        actualValue.ShouldNotBeNull($"Expected structured logging key '{key}' to be present.");

        return record;
    }

    /// <summary>
    /// Asserts that the record contains a structured-state key with an expected string value.
    /// </summary>
    /// <param name="record">The log record to validate.</param>
    /// <param name="key">The expected structured-state key.</param>
    /// <param name="expectedValue">The expected string value for the key.</param>
    /// <returns>The same <paramref name="record"/> to support fluent assertions.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Call: Invoke");
    ///
    /// // Fixture setup + assertion code
    /// var record = collector.ShouldContainLog(r => r.Message == "Call: Invoke");
    /// _ = record.ShouldContainStructuredStateStringValue("MethodName", "Invoke");
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainStructuredStateStringValue(this FakeLogRecord record, string key,
        string expectedValue)
    {
        _ = record.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedValue);

        var actualValue = record.GetStructuredStateValue(key);

        actualValue.ShouldNotBeNull($"Expected structured logging key '{key}' to be present.");
        actualValue.ToString().ShouldBe(expectedValue,
            $"Structured logging key '{key}' did not match the expected string value.");

        return record;
    }

    /// <summary>
    /// Asserts that the record contains a structured-state key with a non-empty string value.
    /// </summary>
    /// <param name="record">The log record to validate.</param>
    /// <param name="key">The expected structured-state key.</param>
    /// <returns>The same <paramref name="record"/> to support fluent assertions.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogInformation("Call: Invoke");
    ///
    /// // Fixture setup + assertion code
    /// var record = collector.ShouldContainLog(r => r.Message == "Call: Invoke");
    /// _ = record.ShouldContainStructuredStateNonEmptyStringValue("correlationId");
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainStructuredStateNonEmptyStringValue(this FakeLogRecord record,
        string key)
    {
        _ = record.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var actualValue = record.GetStructuredStateValue(key);

        actualValue.ShouldNotBeNull($"Expected structured logging key '{key}' to be present.");
        actualValue.ToString().ShouldNotBeNullOrWhiteSpace(
            $"Structured logging key '{key}' should contain a non-empty string value.");

        return record;
    }

    /// <summary>
    /// Asserts a <c>LogCall(..., arguments)</c>-style record with expected category, message shape, level, and
    /// structured-state values.
    /// </summary>
    /// <param name="collector">The fake log collector containing captured records.</param>
    /// <param name="category">The expected logger category name.</param>
    /// <param name="callerName">The expected caller/member name.</param>
    /// <param name="callerType">
    /// Optional caller type used to build method name values for instance-call scenarios. Use <see langword="null"/>
    /// for static-caller scenarios.
    /// </param>
    /// <param name="includeCallerNamespace">Whether the caller type name should include its namespace.</param>
    /// <param name="expectedLevel">The expected log level.</param>
    /// <param name="assertNoAdditionalStructuredState">
    /// When <see langword="true"/>, asserts that no additional structured-state keys exist beyond expected keys.
    /// </param>
    /// <param name="callPrefix">The call prefix used in the rendered message, such as <c>Call: </c>.</param>
    /// <param name="methodNameProperty">The structured-state key used for method name value.</param>
    /// <param name="argumentsPrefix">The text prefix that appears before arguments in the rendered message.</param>
    /// <param name="argumentsProperty">The expected arguments structured-state key name (for example <c>Arguments</c>).</param>
    /// <param name="originalFormatProperty">The structured-state key used for message template value.</param>
    /// <param name="expectedArguments">
    /// Optional expected arguments object. When provided, assertions validate either the object itself (for
    /// <c>Arguments</c>/<c>@Arguments</c>) or its readable property values.
    /// </param>
    /// <param name="assertArguments">Optional callback for custom arguments assertions.</param>
    /// <param name="expectedMessage">Optional exact expected rendered message.</param>
    /// <param name="useMessageStartsWithWhenExpectedMessageNotProvided">
    /// When <see langword="true"/> and <paramref name="expectedMessage"/> is <see langword="null"/>, assertions
    /// validate message prefix instead of exact message equality.
    /// </param>
    /// <param name="customMessage">An optional assertion message used when no matching record is found.</param>
    /// <returns>The first matching <see cref="FakeLogRecord"/>.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer code under test
    /// logger.LogCall(null, new { Username = "alice" }, callerName: "Invoke");
    ///
    /// // Fixture assertion code
    /// _ = collector.ShouldContainLogCallWithArguments(
    ///     category: "Pot.TestUtils.FakeLogging",
    ///     callerName: "Invoke",
    ///     assertNoAdditionalStructuredState: false);
    /// ]]></code>
    /// </example>
    public static FakeLogRecord ShouldContainLogCallWithArguments(this FakeLogCollector collector,
        string category, string callerName, Type? callerType = null, bool includeCallerNamespace = true,
        LogLevel expectedLevel = LogLevel.Information, bool assertNoAdditionalStructuredState = true,
        string callPrefix = "Call: ", string methodNameProperty = "MethodName",
        string argumentsPrefix = "Arguments = ", string argumentsProperty = "Arguments",
        string originalFormatProperty = "{OriginalFormat}", object? expectedArguments = null,
        Action<object?>? assertArguments = null, string? expectedMessage = null,
        bool useMessageStartsWithWhenExpectedMessageNotProvided = true, string? customMessage = null)
    {
        _ = collector.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(callerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(callPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodNameProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(argumentsPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(argumentsProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFormatProperty);

        var methodNameValue = GetExpectedLogCallMethodName(callerName, callerType, includeCallerNamespace);
        var template = GetLogCallWithArgumentsTemplate(callPrefix, methodNameProperty, argumentsPrefix, argumentsProperty);
        var expectedMessagePrefix = $"{callPrefix}{methodNameValue}, {argumentsPrefix}";

        var record = collector.ShouldContainLog(record =>
                string.Equals(record.Category, category, StringComparison.Ordinal)
                && IsExpectedLogCallMessage(record.Message, expectedMessage, expectedMessagePrefix,
                    useMessageStartsWithWhenExpectedMessageNotProvided),
            customMessage ?? $"Expected a LogCall(arguments) record in category '{category}'.");

        record.Level.ShouldBe(expectedLevel, "LogCall(arguments) level did not match the expected level.");

        var prefixedArgumentsProperty = argumentsProperty.StartsWith("@", StringComparison.Ordinal)
            ? argumentsProperty
            : $"@{argumentsProperty}";

        var actualArguments = record.GetStructuredStateValue(argumentsProperty)
            ?? record.GetStructuredStateValue(prefixedArgumentsProperty);

        var expectedStructuredState = new Dictionary<string, object?>
        {
            [methodNameProperty] = methodNameValue,
            [originalFormatProperty] = template
        };

        if (actualArguments is not null)
        {
            if (expectedArguments is not null)
            {
                actualArguments.ShouldBe(expectedArguments,
                    $"Structured logging key '{argumentsProperty}' did not match the expected value.");
            }

            var actualArgumentsKey = record.GetStructuredStateValue(argumentsProperty) is not null
                ? argumentsProperty
                : prefixedArgumentsProperty;

            expectedStructuredState[actualArgumentsKey] = actualArguments;
            assertArguments?.Invoke(actualArguments);
        }
        else if (expectedArguments is not null)
        {
            var expectedArgumentProperties = GetObjectPropertyValues(expectedArguments);

            expectedArgumentProperties.Count.ShouldBeGreaterThan(0,
                "Expected arguments did not expose public properties to validate structured logging values.");

            foreach (var expectedArgumentProperty in expectedArgumentProperties)
            {
                var actualArgumentPropertyValue = record.GetStructuredStateValue(expectedArgumentProperty.Key);

                actualArgumentPropertyValue.ShouldNotBeNull(
                    $"Expected structured logging key '{expectedArgumentProperty.Key}' to be present.");

                actualArgumentPropertyValue.ShouldBe(expectedArgumentProperty.Value,
                    $"Structured logging key '{expectedArgumentProperty.Key}' did not match the expected argument value.");

                expectedStructuredState[expectedArgumentProperty.Key] = expectedArgumentProperty.Value;
            }

            assertArguments?.Invoke(null);
        }
        else
        {
            assertArguments?.Invoke(null);
        }

        record.ShouldHaveStructuredState(expectedStructuredState, assertNoAdditionalStructuredState);

        return record;
    }

    /// <summary>
    /// Builds the expected method-name value used by <c>LogCall</c> structured logging.
    /// </summary>
    /// <param name="callerName">The caller/member name.</param>
    /// <param name="callerType">The caller type for instance-call scenarios; <see langword="null"/> for static scenarios.</param>
    /// <param name="includeCallerNamespace">Whether the caller type name should include its namespace.</param>
    /// <returns>The expected method-name value written to structured state.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer-side expectation setup
    /// var expected = FakeLogCollectorExtensions.GetExpectedLogCallMethodName("Invoke", callerType: null);
    ///
    /// // Fixture assertion code
    /// expected.ShouldBe("Invoke");
    /// ]]></code>
    /// </example>
    public static string GetExpectedLogCallMethodName(string callerName, Type? callerType = null,
        bool includeCallerNamespace = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callerName);

        if (callerType is null)
        {
            return callerName;
        }

        var friendlyTypeName = callerType.GetFriendlyName(includeCallerNamespace);
        var typeAndMethodName = $"{friendlyTypeName}.{callerName}";

        return typeAndMethodName;
    }

    /// <summary>
    /// Builds the expected message template for <c>LogCall(...)</c> without arguments.
    /// </summary>
    /// <param name="callPrefix">The call prefix used in the template.</param>
    /// <param name="methodNameProperty">The structured-state method-name property key.</param>
    /// <returns>The expected template string, for example <c>Call: {MethodName}</c>.</returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer-side expectation setup
    /// var template = FakeLogCollectorExtensions.GetLogCallTemplate();
    ///
    /// // Fixture assertion code
    /// template.ShouldBe("Call: {MethodName}");
    /// ]]></code>
    /// </example>
    public static string GetLogCallTemplate(string callPrefix = "Call: ", string methodNameProperty = "MethodName")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodNameProperty);

        return $"{callPrefix}{{{methodNameProperty}}}";
    }

    /// <summary>
    /// Builds the expected message template for <c>LogCall(..., arguments)</c>.
    /// </summary>
    /// <param name="callPrefix">The call prefix used in the template.</param>
    /// <param name="methodNameProperty">The structured-state method-name property key.</param>
    /// <param name="argumentsPrefix">The text prefix used before the arguments placeholder.</param>
    /// <param name="argumentsProperty">The structured-state arguments property key.</param>
    /// <returns>
    /// The expected template string, for example <c>Call: {MethodName}, Arguments = {@Arguments}</c>.
    /// </returns>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// // Consumer-side expectation setup
    /// var template = FakeLogCollectorExtensions.GetLogCallWithArgumentsTemplate();
    ///
    /// // Fixture assertion code
    /// template.ShouldBe("Call: {MethodName}, Arguments = {@Arguments}");
    /// ]]></code>
    /// </example>
    public static string GetLogCallWithArgumentsTemplate(string callPrefix = "Call: ", string methodNameProperty = "MethodName",
        string argumentsPrefix = "Arguments = ", string argumentsProperty = "Arguments")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodNameProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(argumentsPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(argumentsProperty);

        return $"{callPrefix}{{{methodNameProperty}}}, {argumentsPrefix}{{@{argumentsProperty}}}";
    }

    private static bool IsExpectedLogCallMessage(string actualMessage, string? expectedMessage,
        string expectedMessagePrefix, bool useMessageStartsWithWhenExpectedMessageNotProvided)
    {
        if (!string.IsNullOrWhiteSpace(expectedMessage))
        {
            return string.Equals(actualMessage, expectedMessage, StringComparison.Ordinal);
        }

        if (!useMessageStartsWithWhenExpectedMessageNotProvided)
        {
            return string.Equals(actualMessage, expectedMessagePrefix, StringComparison.Ordinal);
        }

        return actualMessage.StartsWith(expectedMessagePrefix, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, object?> GetObjectPropertyValues(object value)
    {
        _ = value.WhenNotNull();

        var properties = value
            .GetType()
            .GetProperties()
            .Where(property => property.CanRead)
            .ToDictionary(property => property.Name, property => property.GetValue(value), StringComparer.Ordinal);

        return properties;
    }

}