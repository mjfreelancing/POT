using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.OpenApi;

// Documents the members every Problem Details response carries beyond the standard five.
//
// The generated schema describes the properties of the ProblemDetails type and stops there, because everything else
// travels in its Extensions dictionary, whose contents reflection cannot see. So correlationId - the one member a
// caller is told to quote on every failure - is the one member the document does not mention, and a generated client
// has no property for it.
//
// Applied to every type assignable to ProblemDetails rather than to ProblemDetails alone, so a 400 declared as
// HttpValidationProblemDetails, which the generator describes as a schema of its own, is covered too.
internal sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string TraceIdMember = "traceId";
    private const string InstanceMember = "instance";
    private const string ErrorsMember = "errors";

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (!typeof(Microsoft.AspNetCore.Mvc.ProblemDetails).IsAssignableFrom(context.JsonTypeInfo.Type))
        {
            return Task.CompletedTask;
        }

        schema.Properties ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);

        // Added to every body by the shared CustomizeProblemDetails.
        schema.Properties[ProblemDetailsOptionsSetup.CorrelationIdExtension] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Identifies this request in the logs. Quote it when reporting a problem."
        };

        // Added by the framework's writer, from the current Activity's id - the W3C traceparent - or, where there is no
        // Activity, from the request identifier, which is the same value as correlationId. Never null.
        schema.Properties[TraceIdMember] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Identifies this request in distributed tracing: the W3C traceparent where the request was " +
                "traced, otherwise the same value as correlationId."
        };

        // Replaced rather than left as generated: the property is nullable on the type and so documented as
        // string-or-null, but the shared customisation always sets it, and "required but nullable" would make a
        // generated client handle a null that never arrives.
        schema.Properties[InstanceMember] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "The path of the request that failed."
        };

        schema.Required ??= new HashSet<string>(StringComparer.Ordinal);
        schema.Required.Add(ProblemDetailsOptionsSetup.CorrelationIdExtension);
        schema.Required.Add(TraceIdMember);
        schema.Required.Add(InstanceMember);

        // Only a validation problem carries the map, and every 400 carries at least one message in it. The framework's
        // type holds it as a dictionary the generator cannot describe, so the shape is written here.
        if (typeof(HttpValidationProblemDetails).IsAssignableFrom(context.JsonTypeInfo.Type))
        {
            schema.Properties[ErrorsMember] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                },
                Description = "The validation messages, keyed by the field each belongs to. A rejected header is keyed " +
                    "by the name the caller sent, and a message about the whole request by the empty string."
            };

            schema.Required.Add(ErrorsMember);
        }

        return Task.CompletedTask;
    }
}
