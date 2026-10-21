using Microsoft.AspNetCore.OpenApi;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Pot.AspNetCore.Concerns.OpenApi;

// Names each schema component in the OpenAPI document.
//
// Feature slices name their contracts for their role - Request, Response - and rely on the folder for context. The
// document has no folders: components live in one flat namespace keyed by the short type name, and when two types
// ask for the same key one is dropped and every reference points at the other. Nothing reports it - no error, no
// warning, no numbered suffix - so the symptom is an endpoint documenting another feature's body. The namespace
// therefore supplies what the folder does: Pot.AspNetCore.Features.Accounts.Create.Request is Accounts_Create_Request.
//
// A feature type from another layer is qualified by its assembly as well. Pot.App slices by feature and operation too,
// so its Input, Output and Models sit at the same paths as the contracts here, and only naming habit keeps them apart.
// Such a type has no business in the document anyway: App_Projections_Models_DateProjection cannot collide, and reads
// as the leak it is.
//
// SchemaReferenceIdFixture asserts that no two feature types produce the same name.
internal static class SchemaReferenceId
{
    private const string FeaturesNamespacePrefix = "Pot.AspNetCore.Features.";
    private const string FeaturesSegment = "Features";

    private static readonly Assembly ApiAssembly = typeof(SchemaReferenceId).Assembly;

    public static string? Create(JsonTypeInfo jsonTypeInfo)
    {
        var defaultReferenceId = OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);

        // Null means "describe this inline", which the framework returns for primitives and collections. Naming those
        // would give an array of Response a component of its own, keyed "Response[]" - a key OpenAPI does not allow.
        if (defaultReferenceId is null)
        {
            return null;
        }

        var type = jsonTypeInfo.Type;
        var namespaceName = type.Namespace;

        if (namespaceName is null)
        {
            return defaultReferenceId;
        }

        if (type.Assembly == ApiAssembly)
        {
            if (!namespaceName.StartsWith(FeaturesNamespacePrefix, StringComparison.Ordinal))
            {
                return defaultReferenceId;
            }

            var featureName = namespaceName[FeaturesNamespacePrefix.Length..].Replace('.', '_');

            return $"{featureName}_{defaultReferenceId}";
        }

        var segments = namespaceName.Split('.');
        var featuresIndex = Array.IndexOf(segments, FeaturesSegment);

        // A shared or framework type - ProblemDetails, an enum from Pot.Shared. Its default name is already
        // unambiguous, and renaming it would churn the document for nothing.
        if (featuresIndex < 0)
        {
            return defaultReferenceId;
        }

        // The area comes from the assembly rather than the namespace, so a namespace that merely contains a Features
        // segment cannot pass for this assembly's slices. "Pot.App" reads as "App": every assembly shares the prefix.
        var assemblyName = type.Assembly.GetName().Name ?? "Unknown";
        var area = assemblyName[(assemblyName.LastIndexOf('.') + 1)..];

        return string.Join('_', [area, .. segments[(featuresIndex + 1)..], defaultReferenceId]);
    }
}
