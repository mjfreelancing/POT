using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Extensions;
using Shouldly;

namespace Pot.App.Tests.Extensions;

public class LoggerExtensionsFixture
{
    [Fact]
    public void Should_Log_The_Message_And_Property_Only()
    {
        var logger = Substitute.For<ILogger>();

        var error = new ApiDetailError(ErrorType.BadRequest)
        {
            ErrorCode = ErrorCodes.Invalid,
            PropertyName = "PropertyName",
            ErrorMessage = "The value is invalid."
        };

        logger.LogApiError(error);

        var call = logger.ReceivedCalls().ShouldHaveSingleItem();
        var state = call.GetArguments()[2];
        var message = state?.ToString() ?? string.Empty;

        message.ShouldBe("Validation Error: The value is invalid. (Property: PropertyName)");
    }
}
