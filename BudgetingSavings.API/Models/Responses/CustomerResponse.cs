using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.API.Models.Responses
{
    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
