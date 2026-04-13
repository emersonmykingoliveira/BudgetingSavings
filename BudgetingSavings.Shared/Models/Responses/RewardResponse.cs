using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Responses
{
    public class RewardResponse
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int Points { get; set; }
        public bool Redeemed { get; set; }
        public decimal CashBack { get; set; }
        public DateTime? RedeemedDate { get; set; }
        public Guid CustomerId { get; set; }
    }
}
