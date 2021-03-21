using System;

namespace Application.Dto
{
    public class AuthorizeResponse : PaymentResponse
    {
        public AuthorizeResponse(int amount, string currency, Guid transactionId) : base(amount, currency)
        {
            TransactionId = transactionId;
        }

        public Guid TransactionId { get; set; }
    }
}