using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Settings.GetAll.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Settings.GetAll;

internal sealed class Response
{
    internal sealed class SettingItem
    {
        // RowId and Etag will be null when not in database - a default Value will be provided
        [Description("The resource identifier")]
        public Guid? RowId { get; init; }

        [Description("The entity tag for optimistic concurrency")]
        public long? Etag { get; init; }
        [Description("The setting category")]
        public required string Category { get; init; }

        [Description("The setting key")]
        public required string Key { get; init; }

        [Description("The setting value")]
        public required object Value { get; init; }

        [Description("The setting description")]
        public required string Description { get; init; }
    }

    [Description("List of all settings")]
    public required SettingItem[] Settings { get; init; }

    public static Ok<Response> Ok(Output output)
    {
        _ = output.WhenNotNull();

        var settings = from category in output.Categories
                       from setting in category.Settings
                       select new SettingItem
                       {
                           Category = category.Category,
                           Key = setting.Key,
                           RowId = setting.Value.RowId,
                           Etag = setting.Value.Etag,
                           Value = setting.Value.Value,
                           Description = setting.Value.Description
                       };

        var response = new Response
        {
            Settings = [.. settings]
        };

        return TypedResults.Ok(response);
    }
}
