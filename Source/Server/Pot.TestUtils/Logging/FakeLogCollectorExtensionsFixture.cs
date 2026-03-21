using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging.Testing;
using Shouldly;

namespace Pot.TestUtils.Logging;

/// <summary>
/// Contract tests for <see cref="FakeLogCollectorExtensions"/>.
/// These tests exist to validate helper behavior in Pot.TestUtils and are intended
/// to move with the helpers when they are extracted to a dedicated package.
/// </summary>
public class FakeLogCollectorExtensionsFixture
{
    /// <summary>
    /// Verifies method-name resolution semantics used by LogCall-focused helper assertions.
    /// These checks should migrate with the helper package.
    /// </summary>
    public sealed class MethodNameResolution
    {
        /// <summary>
        /// Verifies that method-name resolution returns only the caller name when no caller type is supplied.
        /// </summary>
        [Fact]
        public void Should_Return_CallerName_When_CallerType_Is_Null()
        {
            var actual = FakeLogCollectorExtensions.GetExpectedLogCallMethodName("Invoke");

            actual.ShouldBe("Invoke");
        }

        /// <summary>
        /// Verifies that method-name resolution includes a fully qualified caller type when requested.
        /// </summary>
        [Fact]
        public void Should_Return_NamespaceQualified_TypeAndCallerName_When_CallerType_Is_Provided()
        {
            var actual = FakeLogCollectorExtensions.GetExpectedLogCallMethodName(
                "Execute",
                typeof(SampleCaller<int>),
                includeCallerNamespace: true);

            actual.ShouldBe($"{typeof(SampleCaller<int>).GetFriendlyName(true)}.Execute");
        }

        /// <summary>
        /// Verifies that method-name resolution excludes namespace segments when full qualification is disabled.
        /// </summary>
        [Fact]
        public void Should_Return_TypeAndCallerName_Without_Namespace_When_CallerType_Is_Provided()
        {
            var actual = FakeLogCollectorExtensions.GetExpectedLogCallMethodName(
                "Execute",
                typeof(SampleCaller<int>),
                includeCallerNamespace: false);

            actual.ShouldBe($"{typeof(SampleCaller<int>).GetFriendlyName(false)}.Execute");
        }
    }

    /// <summary>
    /// Verifies end-to-end fake-log assertions against real AllOverIt LogCall output.
    /// These checks should migrate with the helper package.
    /// </summary>
    public sealed class LogCallAssertions
    {
        private const string Category = "Pot.TestUtils.FakeLogging";

        /// <summary>
        /// Verifies helper assertions for static-caller <c>LogCall</c> records.
        /// </summary>
        [Fact]
        public void Should_Validate_LogCall_For_Static_Caller()
        {
            var collector = new FakeLogCollector();
            var logger = new FakeLogger(collector, Category);

            logger.LogCall(null, callerName: "Invoke");

            var actual = collector.ShouldContainLogCall(
                Category,
                callerName: "Invoke",
                callerType: null,
                assertNoAdditionalStructuredState: true);

            actual.Message.ShouldBe("Call: Invoke");
            actual.ShouldContainStructuredStateStringValue("MethodName", "Invoke")
                .ShouldContainStructuredStateStringValue("{OriginalFormat}", "Call: {MethodName}");
        }

        /// <summary>
        /// Verifies helper assertions for instance-caller <c>LogCall</c> records.
        /// </summary>
        [Fact]
        public void Should_Validate_LogCall_For_Instance_Caller()
        {
            var collector = new FakeLogCollector();
            var logger = new FakeLogger(collector, Category);
            var caller = new SampleCaller<string>();

            logger.LogCall(caller, callerName: "Run");

            var expectedMethodName = FakeLogCollectorExtensions.GetExpectedLogCallMethodName(
                "Run",
                caller.GetType(),
                includeCallerNamespace: true);

            var actual = collector.ShouldContainLogCall(
                Category,
                callerName: "Run",
                callerType: caller.GetType(),
                includeCallerNamespace: true,
                assertNoAdditionalStructuredState: true);

            actual.ShouldContainStructuredStateStringValue(
                "MethodName",
                expectedMethodName);
        }

        /// <summary>
        /// Verifies helper assertions for <c>LogCall</c> records that include arguments.
        /// </summary>
        [Fact]
        public void Should_Validate_LogCall_With_Arguments()
        {
            var collector = new FakeLogCollector();
            var logger = new FakeLogger(collector, Category);
            var arguments = new LogArguments("alice", 3);

            logger.LogCall(null, arguments, callerName: "Invoke");

            var actual = collector.ShouldContainLogCallWithArguments(
                Category,
                callerName: "Invoke",
                callerType: null,
                assertNoAdditionalStructuredState: true);

            actual.ShouldContainStructuredStateStringValue("MethodName", "Invoke")
                .ShouldContainStructuredStateStringValue("{OriginalFormat}",
                    "Call: {MethodName}, Arguments = {@Arguments}");

            actual.Message.StartsWith("Call: Invoke, Arguments = ", StringComparison.Ordinal).ShouldBeTrue();
        }
    }

    private sealed record LogArguments(string Username, int RetryCount);

    private sealed class SampleCaller<TValue>
    {
    }
}
