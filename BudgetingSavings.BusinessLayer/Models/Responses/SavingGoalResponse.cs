using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.BusinessLayer.Models.Responses
{
    public class SavingGoalResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public Guid CustomerId { get; set; }
    }
}
