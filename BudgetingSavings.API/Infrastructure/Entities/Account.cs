namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountType { get; set; }
        public decimal Balance { get; set; }
        public string? Currency { get; set; }
        public string? Owner { get; set; }
    }
}
