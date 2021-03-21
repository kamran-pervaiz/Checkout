using System.Collections.Generic;
using Application.Dto;
using Domain.Entities;
using Domain.Enum;

namespace Application.Mappings
{
    public class CustomerMapper : IMapper<CustomerDto, Customer>
    {
        public Customer Map(CustomerDto source)
        {
            return new Customer
            {
                CardNumber = source.CardNumber,
                BankAmount = source.Amount,
                Currency = source.Currency,
                Cvv = source.Cvv,
                ExpiryMonth = source.ExpiryMonth,
                ExpiryYear = source.ExpiryYear,
                Status = Status.CanCapture,
                TransactionHistories = new List<TransactionHistory>
                {
                    new TransactionHistory
                    {
                        Amount = source.Amount,
                        Type = TransactionType.Authorize
                    }
                }
            };
        }
    }
}