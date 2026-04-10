using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Requests
{
    public class CreateAccountRequest
    {
        public string? AccountType { get; set; }
        public string? Currency { get; set; }
        public string? Owner { get; set; }
    }
}
