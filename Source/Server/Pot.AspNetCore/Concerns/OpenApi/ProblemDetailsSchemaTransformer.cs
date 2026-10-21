using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Pot.AspNetCore.Extensions;

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

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (!typeof(Microsoft.AspNetCore.Mvc.ProblemDetails).IsAssignableFrom(context.JsonTypeInfo.Type))
        {
            return Task.CompletedTask;
        }

        schema.Properties ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);

        // Added to every body by the shared CustomizeProblemDetails.
        schema.Properties[WebApplicationBuilderExtensions.CorrelationIdExtension] = new OpenApiSchema
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
        schema.Required.Add(WebApplicationBuilderExtensions.CorrelationIdExtension);
        schema.Required.Add(TraceIdMember);
        schema.Required.Add(InstanceMember);

        return Task.CompletedTask;
    }
}
