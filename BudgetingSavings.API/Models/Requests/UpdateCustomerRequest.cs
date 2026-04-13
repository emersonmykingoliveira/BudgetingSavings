using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Requests
{
    public class UpdateCustomerRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
