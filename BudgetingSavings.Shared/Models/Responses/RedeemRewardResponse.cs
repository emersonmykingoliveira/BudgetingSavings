using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Responses
{
    public class RedeemRewardResponse
    {
        public int RedeemedPoints { get; set; }
        public decimal CashBack { get; set; }
        public decimal UpdatedAccountBalance { get; set; }
        public Guid AccountId { get; set; }
        public Guid CustomerId { get; set; }
    }
}
