using BudgetingSavings.API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.Shared.Models.Requests
{
    public class TransferRequest
    {
        public Guid AccountOriginId { get; set; }
        public Guid AccountDestinationId { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
