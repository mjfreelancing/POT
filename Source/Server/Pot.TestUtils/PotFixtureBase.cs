using AllOverIt.Fixture;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Pot.Shared.Enumerations;
using System.Diagnostics.CodeAnalysis;

namespace Pot.TestUtils;

[ExcludeFromCodeCoverage]
public abstract class PotFixtureBase : FixtureBase
{
    /// <summary>Default constructor.</summary>
    public PotFixtureBase()
    {
        var customization = new AutoNSubstituteCustomization { GenerateDelegates = true };
        Customize(customization);
    }

    public void CustomizeEnumerations()
    {
        Customize<UserStatus>();
        Customize<OtpReason>();
        Customize<OtpStatus>();
        Customize<Role>();
        Customize<Permission>();
        Customize<SettingCategory>();
        Customize<Frequency>();
    }

    public void OmitRecursionBehavior()
    {
        // Configure AutoFixture to handle circular references in entity relationships (bidirectional navigation properties)
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(behaviour => Fixture.Behaviors.Remove(behaviour));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
