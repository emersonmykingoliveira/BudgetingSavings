using BudgetingSavings.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Responses
{
    public class SavingGoalStatusResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public SavingGoalStatus IsCompleted { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public int? DaysRemaining { get; set; }
        public Guid CustomerId { get; set; }
    }
}
