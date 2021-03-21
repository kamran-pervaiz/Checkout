using System.Collections.Generic;
using Domain.Enum;

namespace Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public int Cvv { get; set; }
        public int BankAmount { get; set; } //Treating it as actual bank amount for simplicity
        public string Currency { get; set; }
        public Status Status { get; set; }
        public ICollection<TransactionHistory> TransactionHistories { get; set; }
    }
}