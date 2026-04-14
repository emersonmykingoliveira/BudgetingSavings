using BudgetingSavings.API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.API.Models.Requests
{
    public class CreateAccountRequest
    {
        public AccountType AccountType { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid CustomerId { get; set; }
    }
}
