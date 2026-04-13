using System.Text.Json.Serialization;

namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public List<Account>? Accounts { get; set; }
        public List<SavingGoal>? SavingGoals { get; set; }
        public List<Budget>? Budgets { get; set; }
        public List<Reward>? Rewards { get; set; }
    }
}
