using BudgetingSavings.API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Responses
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionCategory TransactionCategory { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid CustomerId { get; set; }
        public Guid AccountId { get; set; }
    }
}
