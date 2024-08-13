using Pot.Maui.Domain.Expenses.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.Maui.Domain.Expenses.Repository
{
    public interface IExpenseRepository
    {
        Task<Expense[]> GetAllExpensesAsync();
    }

    internal sealed class ExpenseRepository : IExpenseRepository
    {
        public async Task<Expense[]> GetAllExpensesAsync()
        {
            var expensesJson = await LoadMauiAsset("expenses.json");

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<Expense[]>(expensesJson, options)!;
        }

        async Task<string> LoadMauiAsset(string assetName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetName);
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }
    }
}
