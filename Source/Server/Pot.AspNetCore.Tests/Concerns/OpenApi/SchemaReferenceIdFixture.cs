using Pot.App.Features.Projections.Models;
using Pot.AspNetCore.Concerns.OpenApi;
using Shouldly;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Tests.Concerns.OpenApi;

public class SchemaReferenceIdFixture
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public class Create : SchemaReferenceIdFixture
    {
        [Fact]
        public void Should_Qualify_An_Api_Feature_Type_By_Its_Slice()
        {
            SchemaReferenceId.Create(GetTypeInfo(typeof(Pot.AspNetCore.Features.Accounts.Create.Request)))
                .ShouldBe("Accounts_Create_Request");
        }

        [Fact]
        public void Should_Qualify_Another_Layers_Feature_Type_By_Its_Assembly_And_Slice()
        {
            SchemaReferenceId.Create(GetTypeInfo(typeof(DateProjection)))
                .ShouldBe("App_Projections_Models_DateProjection");
        }

        [Fact]
        public void Should_Keep_The_Default_Name_For_A_Type_Outside_A_Feature()
        {
            SchemaReferenceId.Create(GetTypeInfo(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails)))
                .ShouldBe("ProblemDetails");
        }

        [Fact]
        public void Should_Inline_A_Collection_Of_A_Feature_Type()
        {
            // A component of its own would be keyed "Response[]", which OpenAPI does not allow.
            SchemaReferenceId.Create(GetTypeInfo(typeof(Pot.AspNetCore.Features.Accounts.GetAll.Response[]))).ShouldBeNull();
            SchemaReferenceId.Create(GetTypeInfo(typeof(IReadOnlyCollection<DateProjection>))).ShouldBeNull();
        }

        [Fact]
        public void Should_Give_Every_Feature_Type_A_Distinct_Name()
        {
            // The document keeps one schema per name and drops the other without reporting it, so a clash shows up
            // only as an endpoint documenting another feature's body. Every feature type is checked, not only those in
            // the document today, so a clash fails here before a new endpoint brings the second type in.
            var featureTypeNames = GetFeatureTypeNames();

            // Without this an empty result passes: a reflection filter that stops matching anything would turn the
            // test into a green one that checks nothing.
            featureTypeNames.ShouldNotBeEmpty();

            var clashes = featureTypeNames
                .GroupBy(item => item.Name, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.Key}: {string.Join(", ", group.Select(item => item.Type.FullName))}")
                .ToList();

            clashes.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Name_Every_Feature_Type_With_Characters_OpenApi_Allows()
        {
            var featureTypeNames = GetFeatureTypeNames();

            featureTypeNames.ShouldNotBeEmpty();

            var invalidNames = featureTypeNames
                .Select(item => item.Name)
                .Where(name => !Regex.IsMatch(name, "^[a-zA-Z0-9.\\-_]+$"))
                .ToList();

            invalidNames.ShouldBeEmpty();
        }
    }

    private static JsonTypeInfo GetTypeInfo(Type type)
    {
        return SerializerOptions.GetTypeInfo(type);
    }

    private static List<(Type Type, string Name)> GetFeatureTypeNames()
    {
        Assembly[] assemblies = [typeof(SchemaReferenceId).Assembly, typeof(DateProjection).Assembly];

        var names = new List<(Type Type, string Name)>();

        var featureTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Namespace?.Split('.').Contains("Features") == true)
            .Where(type => type is { IsAbstract: false, IsInterface: false, ContainsGenericParameters: false })
            .Where(type => !type.Name.Contains('<') && !typeof(Delegate).IsAssignableFrom(type));

        foreach (var type in featureTypes)
        {
            JsonTypeInfo typeInfo;

            try
            {
                typeInfo = GetTypeInfo(type);
            }
            catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or ArgumentException)
            {
                // A type the serializer cannot describe - a handler holding a delegate, say - cannot be in the
                // document either, so it cannot clash there.
                continue;
            }

            var name = SchemaReferenceId.Create(typeInfo);

            if (name is not null)
            {
                names.Add((type, name));
            }
        }

        return names;
    }
}
