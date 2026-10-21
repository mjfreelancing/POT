namespace Pot.AspNetCore.Concerns.ProblemDetails;

// Writes every error as Problem Details, whatever media types the request says it accepts.
//
// The framework's writer declines a request whose Accept header excludes JSON - text/html, say, which a browser
// navigation sends - and each route into an error body then falls back differently:
//  - IProblemDetailsService.WriteAsync throws. From CorrelationIdMiddleware that turns a rejected id's 400 into a 500.
//  - TryWriteAsync returns false, so the exception handler middleware and DatabaseExceptionHandler have nothing to
//    write, and UseStatusCodePages writes plain text instead.
//  - TypedResults.Problem serialises the body itself, skipping CustomizeProblemDetails and with it the correlation id.
// A client's Accept header should not decide whether an error can be parsed, and answering with 406 would only
// replace one failure with another.
//
// This wraps the framework's writer rather than replacing it. Everything about the body - the defaults, traceId, the
// customisation, the serializer settings, the content type - is still the framework's; only the decision to write
// is ours.
internal sealed class UnconditionalProblemDetailsWriter : IProblemDetailsWriter
{
    private readonly IProblemDetailsWriter _frameworkWriter;

    public UnconditionalProblemDetailsWriter(IProblemDetailsWriter frameworkWriter)
    {
        _frameworkWriter = frameworkWriter;
    }

    public bool CanWrite(ProblemDetailsContext context)
    {
        return true;
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        // The framework writer's WriteAsync does not re-check CanWrite, so it writes whatever the request accepts.
        // The non-JSON tests in CorrelationAndProblemDetailsFixture and ClientDisconnectFixture depend on that.
        return _frameworkWriter.WriteAsync(context);
    }
}
