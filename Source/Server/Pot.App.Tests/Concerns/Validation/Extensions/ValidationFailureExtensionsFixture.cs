using FluentValidation.Results;
using Pot.App.Concerns.Validation.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Concerns.Validation.Extensions;

public class ValidationFailureExtensionsFixture : PotFixtureBase
{
    public class AddCustomState : ValidationFailureExtensionsFixture
    {
        private readonly ValidationFailure _validationFailure;

        public AddCustomState()
        {
            _validationFailure = new ValidationFailure("TestProperty", "Test error message");
        }

        [Fact]
        public void Should_Initialize_CustomState_When_Null()
        {
            _validationFailure.CustomState.ShouldBeNull();

            _validationFailure.AddCustomState("key1", "value1");

            _validationFailure.CustomState.ShouldNotBeNull();
            _validationFailure.CustomState.ShouldBeOfType<Dictionary<string, object?>>();
        }

        [Fact]
        public void Should_Add_Single_Property_To_CustomState()
        {
            var propertyName = "UserId";
            var value = 123;

            _validationFailure.AddCustomState(propertyName, value);

            var customState = _validationFailure.CustomState as Dictionary<string, object?>;
            customState!.ShouldContainKey(propertyName);
            customState![propertyName].ShouldBe(value);
        }

        [Fact]
        public void Should_Add_Multiple_Properties_To_CustomState()
        {
            _validationFailure.AddCustomState("Property1", "Value1");
            _validationFailure.AddCustomState("Property2", 42);
            _validationFailure.AddCustomState("Property3", true);

            var customState = _validationFailure.CustomState as Dictionary<string, object?>;
            customState!.Count.ShouldBe(3);
            customState!["Property1"].ShouldBe("Value1");
            customState["Property2"].ShouldBe(42);
            customState["Property3"].ShouldBe(true);
        }

        [Fact]
        public void Should_Add_Null_Value_To_CustomState()
        {
            var propertyName = "NullableProperty";

            _validationFailure.AddCustomState(propertyName, null);

            var customState = _validationFailure.CustomState as Dictionary<string, object?>;
            customState!.ShouldContainKey(propertyName);
            customState![propertyName].ShouldBeNull();
        }

        [Fact]
        public void Should_Handle_Various_Value_Types()
        {
            _validationFailure.AddCustomState("StringValue", "test");
            _validationFailure.AddCustomState("IntValue", 123);
            _validationFailure.AddCustomState("BoolValue", true);
            _validationFailure.AddCustomState("DateTimeValue", new DateTime(2026, 1, 28));
            _validationFailure.AddCustomState("GuidValue", Guid.NewGuid());

            var customState = _validationFailure.CustomState as Dictionary<string, object?>;
            customState!.Count.ShouldBe(5);
        }

        [Fact]
        public void Should_Preserve_Existing_CustomState_When_Adding_New_Properties()
        {
            _validationFailure.CustomState = new Dictionary<string, object?>
            {
                { "ExistingKey", "ExistingValue" }
            };

            _validationFailure.AddCustomState("NewKey", "NewValue");

            var customState = _validationFailure.CustomState as Dictionary<string, object?>;
            customState!.Count.ShouldBe(2);
            customState!["ExistingKey"].ShouldBe("ExistingValue");
            customState["NewKey"].ShouldBe("NewValue");
        }

        [Fact]
        public void Should_Throw_When_Duplicate_Key_Added()
        {
            _validationFailure.AddCustomState("DuplicateKey", "Value1");

            var exception = Should.Throw<ArgumentException>(() =>
            {
                _validationFailure.AddCustomState("DuplicateKey", "Value2");
            });

            exception.Message.ShouldContain("key");
            exception.Message.ShouldContain("already");
        }
    }
}
