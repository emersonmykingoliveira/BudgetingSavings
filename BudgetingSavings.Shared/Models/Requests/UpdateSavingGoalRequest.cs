using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Requests
{
    public class UpdateSavingGoalRequest
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime TargetDate { get; set; }
    }
}
