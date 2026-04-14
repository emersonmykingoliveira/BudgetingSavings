using BudgetingSavings.BusinessLayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.BusinessLayer.Models.Responses
{
    public class BudgetStatusResponse
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsExceeded { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid CustomerId { get; set; }
    }
}
