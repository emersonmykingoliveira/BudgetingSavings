using BudgetingSavings.BusinessLayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetingSavings.BusinessLayer.Models.Responses
{
    public class TransferResponse
    {
        public Guid AccountOriginId { get; set; }
        public Guid AccountDestinyId { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public DateTime Date { get; set; }
    }
}
