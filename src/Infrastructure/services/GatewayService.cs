using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Dto;
using Application.Exceptions;
using Application.Mappings;
using Domain.Entities;
using Domain.Enum;

namespace Infrastructure.services
{
    public class GatewayService : IGatewayService
    {
        private readonly IRepository repository;

        public GatewayService(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<AuthorizeResponse> AuthorizeCustomer(CustomerDto customerDto)
        {
            if (IsCustomerBlackListed(customerDto.CardNumber))
            {
                throw new ValidationException("Authorization failed, card is blacklisted");
            }

            var customer = new CustomerMapper().Map(customerDto);
            await repository.AddAsync(customer);
            return new AuthorizeResponse(customerDto.Amount, customerDto.Currency, customer.Id);
        }

        public async Task<PaymentResponse> Capture(TransactionDto transactionDto)
        {
            var customer = await repository.GetByIdWithIncludeAsync<Customer>(
                transactionDto.TransactionId, new[] { Constants.TransactionHistories });

            if (customer.Status == Status.Refunded || customer.Status == Status.Void)
            {
                throw new ValidationException("transaction cannot be captured anymore");
            }

            var authorizeAmount = GetAmount(customer, TransactionType.Authorize);
            var previousCaptureAmount = GetAmount(customer, TransactionType.Capture);

            customer.Status = Status.CanRefund;
            customer.BankAmount = authorizeAmount - (previousCaptureAmount + transactionDto.Amount);
            customer.TransactionHistories.Add(new TransactionHistory
            {
                Amount = transactionDto.Amount,
                Type = TransactionType.Capture
            });
           

            await repository.UpdateAsync(customer);

            return new PaymentResponse(customer.BankAmount, customer.Currency);
        }

        public async Task<PaymentResponse> Refund(TransactionDto transactionDto)
        {
            var customer = await repository.GetByIdWithIncludeAsync<Customer>(
                transactionDto.TransactionId, new[] { Constants.TransactionHistories });

            if (customer.Status == Status.Refunded || customer.Status == Status.Void)
            {
                throw new ValidationException("cannot refund anymore");
            }

            var authorizedAmount = GetAmount(customer, TransactionType.Authorize);
            var captureAmount = GetAmount(customer, TransactionType.Capture);
            var previousRefundAmount = GetAmount(customer, TransactionType.Refund);

            if (previousRefundAmount + transactionDto.Amount > captureAmount)
            {
                throw new ValidationException("Invalid refund request, Refund amount exceeds capture amount");
            }

            //full refund and no more refund
            if (previousRefundAmount + transactionDto.Amount == captureAmount)
            {
                customer.Status = Status.Refunded;
            }

            customer.BankAmount = (authorizedAmount - captureAmount) + previousRefundAmount + transactionDto.Amount;
            customer.TransactionHistories.Add(new TransactionHistory
            {
                Amount = transactionDto.Amount,
                Type = TransactionType.Refund
            });

            await repository.UpdateAsync(customer);

            return new PaymentResponse(customer.BankAmount, customer.Currency);
        }

        public async Task<PaymentResponse> Cancel(TransactionDto transactionDto)
        {
            var customer = await repository.GetByIdWithIncludeAsync<Customer>(
                transactionDto.TransactionId, new[] { Constants.TransactionHistories });

            if (customer.Status == Status.Void)
            {
                throw new ArgumentException("cannot Cancel a cancelled transaction");
            }

            var authorizeAmount = GetAmount(customer, TransactionType.Authorize);

            //restore the original amount
            customer.BankAmount = authorizeAmount;
            customer.Status = Status.Void;
            customer.TransactionHistories.Add(
                new TransactionHistory
                {
                    Amount = customer.BankAmount,
                    Type = TransactionType.Void
                }
            );

            await repository.UpdateAsync(customer);

            return new PaymentResponse(customer.BankAmount, customer.Currency);
        }

        private static int GetAmount(Customer customer, TransactionType transactionType)
        {
            return customer.TransactionHistories.Where(p => p.Type == transactionType)
                .Sum(p => p.Amount);
        }

        private static bool IsCustomerBlackListed(string creditNumber)
        {
            //this should be coming from database but for simplicity adding in a list
            var blackListCards = new List<string> { "4000 0000 0000 0119" };

            var match = blackListCards.FirstOrDefault(s => s.Contains(creditNumber));

            return match != null;
        }
    }
}