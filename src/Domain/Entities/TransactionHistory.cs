using Domain.Enum;

namespace Domain.Entities
{
    public class TransactionHistory : BaseEntity
    {
        public TransactionType Type { get; set; }

        public int Amount { get; set; }
        //public ICollection<Customer> Customers { get; set; }
    }
}