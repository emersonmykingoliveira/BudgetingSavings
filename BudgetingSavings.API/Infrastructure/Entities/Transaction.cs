using System.Text.Json.Serialization;
using BudgetingSavings.Shared.Models.Enums;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionCategory TransactionCategory { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public Guid AccountId { get; set; }
        public Account? Account { get; set; }
    }
}
