using System.Text.Json.Serialization;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class SavingGoal
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
