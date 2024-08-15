using Pot.Maui.Domain.Expenses.Models;
using Pot.Maui.Helpers;
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
            var expensesJson = await AssetLoader.GetAsStringAsync("expenses.json");

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<Expense[]>(expensesJson, options)!;
        }
    }
}
