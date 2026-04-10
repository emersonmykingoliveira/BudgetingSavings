using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Responses
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public string? AccountNumber { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public CurrencyType Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public Guid CustomerId { get; set; }
    }
}
