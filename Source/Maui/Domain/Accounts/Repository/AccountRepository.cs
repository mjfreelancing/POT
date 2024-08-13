using Pot.Maui.Domain.Accounts.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.Maui.Domain.Accounts.Repository
{
    public interface IAccountRepository
    {
        Task<Account[]> GetAllAccountsAsync();
    }

    internal sealed class AccountRepository : IAccountRepository
    {
        public async Task<Account[]> GetAllAccountsAsync()
        {
            var accountsJson = await LoadMauiAsset("accounts.json");

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<Account[]>(accountsJson, options)!;
        }

        private async Task<string> LoadMauiAsset(string assetName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetName);
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }
    }
}
