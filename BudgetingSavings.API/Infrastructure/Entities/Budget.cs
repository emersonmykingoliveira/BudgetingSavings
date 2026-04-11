using BudgetingSavings.Shared.Models;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Budget
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LimitAmount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
