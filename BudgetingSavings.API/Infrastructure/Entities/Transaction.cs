using BudgetingSavings.Shared.Models;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;
    }
}
