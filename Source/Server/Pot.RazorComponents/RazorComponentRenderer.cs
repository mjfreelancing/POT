using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pot.RazorComponents;

internal class RazorComponentRenderer : IRazorComponentRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public RazorComponentRenderer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    //  ParameterView.Empty when no params required
    public async Task<string> RenderToHtmlAsync<TComponent>(IDictionary<string, object?> parameters) where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);

        // https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-components-outside-of-aspnetcore
        // Any calls to RenderComponentAsync must be made in the context of calling InvokeAsync on a component dispatcher.
        // A component dispatcher is available from the HtmlRenderer.Dispatcher property.
        //Alternatively, you can write the HTML to a TextWriter by calling output.WriteHtmlTo(textWriter).
        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var razorParams = ParameterView.FromDictionary(parameters);
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(razorParams);

            return output.ToHtmlString();
        });
    }
}
