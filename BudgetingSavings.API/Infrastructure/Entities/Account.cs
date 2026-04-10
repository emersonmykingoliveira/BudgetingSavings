namespace BudgetingSavings.API.Infrastructure.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = default!;
        public string AccountType { get; set; } = default!;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = default!;
        public string Owner { get; set; } = default!;
    }
}
