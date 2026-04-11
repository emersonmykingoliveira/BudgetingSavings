using BudgetingSavings.Shared.Models;
using System.ComponentModel;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Budget
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LimitAmount { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
