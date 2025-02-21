using AllOverIt.Extensions;

namespace Pot.AspNetCore.Utils;

public static class ApiQueryStringSerializer
{
    // Converts a TType to an API query string
    public static string ToApiQueryString<TType>(this TType instance) where TType : class
    {
        var properties = instance.ToSerializedDictionary();

        return string.Join("&", properties.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    // Converts an API query string to a TType
    public static void FromApiQueryString<TType>(this TType instance, string queryString) where TType : class
    {
        var properties = queryString
            .Split('&')
            .Select(pair => pair.Split('='))
            .ToDictionary(parts => parts[0], parts => parts[1]);

        foreach (var kvp in properties)
        {
            instance.SetPropertyPathValue(kvp.Key, kvp.Value);
        }
    }
}
