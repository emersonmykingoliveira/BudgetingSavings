using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.BusinessLayer.Models.Requests
{
    public class RedeemRewardRequest
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
    }
}
