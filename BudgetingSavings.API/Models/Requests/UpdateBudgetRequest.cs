using BudgetingSavings.API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.API.Models.Requests
{
    public class UpdateBudgetRequest
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LimitAmount { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
