using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.TestUtils;
using Shouldly;
using System.Text.Json;

namespace Pot.App.Tests.Extensions;

public class ApiDetailsErrorExtensionsFixture : PotFixtureBase
{
    private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

    public class GetErrorDetails_Basic : ApiDetailsErrorExtensionsFixture
    {
        [Fact]
        public void Should_Return_Error_Code_And_Message()
        {
            var error = new ApiBasicError(ErrorType.BadRequest)
            {
                ErrorCode = "SomeCode",
                ErrorMessage = "Some message"
            };

            var keys = GetKeys(error.GetErrorDetails());

            keys.ShouldBe(["errorCode", "errorMessage"], ignoreOrder: true);
        }

        [Fact]
        public void Should_Serialize_Keys_As_CamelCase()
        {
            var error = new ApiBasicError(ErrorType.BadRequest)
            {
                ErrorCode = "SomeCode",
                ErrorMessage = "Some message"
            };

            var serialized = Serialize(error.GetErrorDetails());

            serialized["errorCode"].GetString().ShouldBe("SomeCode");
            serialized["errorMessage"].GetString().ShouldBe("Some message");
        }

        [Fact]
        public void Should_Serialize_Custom_State_Keys_As_CamelCase()
        {
            var error = new ApiBasicError(ErrorType.BadRequest)
            {
                ErrorCode = "SomeCode",
                ErrorMessage = "Some message",
                CustomState = new Dictionary<string, object?>
                {
                    { "MinimumLength", 8 }
                }
            };

            var serialized = Serialize(error.GetErrorDetails());

            serialized.Keys.ShouldBe(["minimumLength", "errorCode", "errorMessage"], ignoreOrder: true);
            serialized["minimumLength"].GetInt32().ShouldBe(8);
        }
    }

    public class GetErrorDetails_Detail : ApiDetailsErrorExtensionsFixture
    {
        [Fact]
        public void Should_Return_All_Error_Properties()
        {
            var error = CreateDetailError();

            var keys = GetKeys(error.GetErrorDetails());

            keys.ShouldBe(["errorCode", "propertyName", "attemptedValue", "errorMessage"], ignoreOrder: true);
        }

        [Fact]
        public void Should_Serialize_Keys_As_CamelCase()
        {
            var error = CreateDetailError();

            var serialized = Serialize(error.GetErrorDetails());

            serialized["errorCode"].GetString().ShouldBe("TooShort");
            serialized["propertyName"].GetString().ShouldBe("NewPassword");
            serialized["attemptedValue"].GetString().ShouldBe("abc");
            serialized["errorMessage"].GetString().ShouldBe("Too short");
        }

        [Fact]
        public void Should_Serialize_Custom_State_Keys_As_CamelCase()
        {
            var error = CreateDetailError(new Dictionary<string, object?>
            {
                { "MinimumLength", 8 }
            });

            var serialized = Serialize(error.GetErrorDetails());

            serialized["minimumLength"].GetInt32().ShouldBe(8);
            serialized["errorCode"].GetString().ShouldBe("TooShort");
        }

        // The defect this fixture exists for: the two branches used to produce different casing, so a client
        // reading errors[].errorCode silently found nothing whenever the error carried custom state.
        [Fact]
        public void Should_Use_The_Same_Keys_With_And_Without_Custom_State()
        {
            var withoutCustomState = Serialize(CreateDetailError().GetErrorDetails());

            var withCustomState = Serialize(CreateDetailError(new Dictionary<string, object?>
            {
                { "MinimumLength", 8 }
            }).GetErrorDetails());

            withCustomState.Keys.ShouldBe([.. withoutCustomState.Keys, "minimumLength"], ignoreOrder: true);
        }

        [Fact]
        public void Should_Preserve_A_Null_Attempted_Value()
        {
            var error = new ApiDetailError(ErrorType.BadRequest)
            {
                ErrorCode = "TooShort",
                PropertyName = "NewPassword",
                AttemptedValue = null,
                ErrorMessage = "Too short"
            };

            var serialized = Serialize(error.GetErrorDetails());

            serialized["attemptedValue"].ValueKind.ShouldBe(JsonValueKind.Null);
        }

        [Fact]
        public void Should_Ignore_Custom_State_That_Is_Not_A_Dictionary()
        {
            var error = CreateDetailError("not a dictionary");

            var keys = GetKeys(error.GetErrorDetails());

            keys.ShouldBe(GetKeys(CreateDetailError().GetErrorDetails()), ignoreOrder: true);
        }

        // Add() throws on a duplicate key, and this runs while an error response is being written.
        [Fact]
        public void Should_Not_Let_Custom_State_Shadow_The_Error_Properties()
        {
            var error = CreateDetailError(new Dictionary<string, object?>
            {
                { "errorCode", "FromCustomState" }
            });

            var serialized = Serialize(error.GetErrorDetails());

            serialized["errorCode"].GetString().ShouldBe("TooShort");
        }

        private static ApiDetailError CreateDetailError(object? customState = null)
        {
            return new ApiDetailError(ErrorType.BadRequest)
            {
                ErrorCode = "TooShort",
                PropertyName = "NewPassword",
                AttemptedValue = "abc",
                ErrorMessage = "Too short",
                CustomState = customState
            };
        }
    }

    private static Dictionary<string, JsonElement> Serialize(object errorDetails)
    {
        var json = JsonSerializer.Serialize(errorDetails, WebOptions);

        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, WebOptions)!;
    }

    private static IReadOnlyCollection<string> GetKeys(object errorDetails)
    {
        return [.. Serialize(errorDetails).Keys];
    }
}
