using System.Text.Json.Serialization;
using BudgetingSavings.BusinessLayer.Models.Enums;

namespace BudgetingSavings.BusinessLayer.Infrastructure.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string? AccountNumber { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public CurrencyType Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public List<Transaction>? Transactions { get; set; }
    }
}
