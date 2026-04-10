using System.Text.Json.Serialization;
using BudgetingSavings.Shared.Models;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string? AccountNumber { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public CurrencyType Currency { get; set; }
        public string? Owner { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastTransactionDate { get; set; }

        [JsonIgnore]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        [JsonIgnore]
        public List<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();
    }
}
