using BudgetingSavings.API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.API.Models.Responses
{
    public class SavingGoalStatusResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public SavingGoalStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public int? DaysRemaining { get; set; }
        public Guid CustomerId { get; set; }
    }
}
