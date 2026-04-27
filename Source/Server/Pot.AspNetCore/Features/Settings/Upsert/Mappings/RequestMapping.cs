using Pot.App.Features.Settings.Upsert.Models;
using Pot.AspNetCore.Features.Settings.Upsert;
using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Features.Settings.Upsert.Mappings;

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
