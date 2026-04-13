using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Responses
{
    public class SavingSuggestionsResponse
    {
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Disposable { get; set; }
        public decimal SavingPercentage { get; set; }
        public decimal RecommendedMontlySaving { get; set; }
        public int ExtimatedMonths { get; set; }
        public Guid CustomerId { get; set; }
    }
}
