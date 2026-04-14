using BudgetingSavings.BusinessLayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.BusinessLayer.Models.Requests
{
    public class CreateAccountRequest
    {
        public AccountType AccountType { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid CustomerId { get; set; }
    }
}
