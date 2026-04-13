using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Requests
{
    public class CreateSavingGoalRequest
    {
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime TargetDate { get; set; }
        public Guid AccountId { get; set; }
        public Guid CustomerId { get; set; }
    }
}
