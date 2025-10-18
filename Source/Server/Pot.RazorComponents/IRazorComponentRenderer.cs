using Microsoft.AspNetCore.Components;

namespace Pot.RazorComponents;

public interface IRazorComponentRenderer
{
    Task<string> RenderToHtmlAsync<TComponent>(IDictionary<string, object?> parameters) where TComponent : IComponent;
}
