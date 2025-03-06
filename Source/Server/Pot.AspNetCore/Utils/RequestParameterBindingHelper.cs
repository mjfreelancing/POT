using AllOverIt.Extensions;

namespace Pot.AspNetCore.Utils;

public static class RequestParameterBindingHelper
{
    /// <summary>Creates a new <typeparamref name="TRequest"/> and sets the property values from the HttpContext query string.
    /// Nested objects are referenced using a dot notation, such as 'Paging.Limit'.</summary>
    /// <typeparam name="TRequest">The request type to be initialised from the HttpContext query string.</typeparam>
    /// <param name="context">The request HttpContext.</param>
    /// <returns>The specified request type.</returns>
    public static TRequest CreateFromQueryString<TRequest>(this HttpContext context) where TRequest : class, new()
    {
        var parameter = new TRequest();

        foreach (var queryItem in context.Request.Query)
        {
            parameter.SetPropertyPathValue(queryItem.Key, queryItem.Value.First());
        }

        return parameter;
    }
}
