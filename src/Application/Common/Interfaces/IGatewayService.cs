using System.Threading.Tasks;
using Application.Dto;

namespace Application.Common.Interfaces
{
    public interface IGatewayService
    {
        Task<AuthorizeResponse> AuthorizeCustomer(CustomerDto customerDto);
        Task<PaymentResponse> Capture(TransactionDto transactionDto);
        Task<PaymentResponse> Refund(TransactionDto transactionDto);
        Task<PaymentResponse> Cancel(TransactionDto transactionDto);
    }
}