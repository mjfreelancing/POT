using Pot.Helpers;
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
            var accountsJson = await AssetLoader.GetAsStringAsync("accounts.json");

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<Account[]>(accountsJson, options)!;
        }
    }
}
