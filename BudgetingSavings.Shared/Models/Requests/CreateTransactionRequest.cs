using BudgetingSavings.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Requests
{
    public class CreateTransactionRequest
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid AccountId { get; set; }
        public Guid CustomerId { get; set; }
    }
}
