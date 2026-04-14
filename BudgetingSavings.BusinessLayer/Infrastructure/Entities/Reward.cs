namespace BudgetingSavings.BusinessLayer.Infrastructure.Entities
{
    public class Reward
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Points { get; set; }
        public bool Redeemed { get; set; }
        public decimal CashBack { get; set; }
        public DateTime? RedeemedDate { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
