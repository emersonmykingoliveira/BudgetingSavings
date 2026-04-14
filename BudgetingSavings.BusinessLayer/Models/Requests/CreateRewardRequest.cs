using BudgetingSavings.BusinessLayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.BusinessLayer.Models.Requests
{
    public class CreateRewardRequest
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionCategory TransactionCategory { get; set; }
    }
}
