using Pot.App.Features.Settings.Update.Models;
using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Features.Settings.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, string category, string key)
    {
        return new Input
        {
            Category = SettingCategory.From(category),
            Key = key,
            Value = request.Value,
            Etag = request.Etag
        };
    }
}
