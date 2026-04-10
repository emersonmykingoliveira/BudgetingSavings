using System.Text.Json.Serialization;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class SavingGoal
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime TargetDate { get; set; }
        public Guid AccountId { get; set; }

        [JsonIgnore]
        public Account? Account { get; set; }
    }
}
