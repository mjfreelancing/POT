namespace Pot.AspNetCore.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        // Alternative to the default SimpleConsole Logger
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
        //
        //builder.Logging.AddJsonConsole(options =>
        //{
        //    options.IncludeScopes = false;
        //    options.TimestampFormat = "HH:mm:ss";
        //    options.JsonWriterOptions = new JsonWriterOptions
        //    {
        //        Indented = true
        //    };
        //});

        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
            options.UseUtcTimestamp = true;
        });

        return builder;
    }
}
