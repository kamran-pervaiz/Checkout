using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Dto;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.services;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Infrastructure.UnitTests.services
{
    [TestFixture]
    public class GatewayServiceTests
    {
        private Mock<IRepository> mockRepository;
        private IGatewayService systemUnderTest;

        private static Customer GetCustomer(int amount, Status status = Status.CanRefund)
        {
            var customer = new Customer
            {
                BankAmount = amount,
                CardNumber = "1234567898765432",
                Currency = "gbp",
                Cvv = 123,
                ExpiryMonth = DateTime.Now.Month.ToString(),
                ExpiryYear = DateTime.Now.Year.ToString(),
                Status = status
            };
            return customer;
        }

        private static void AddTransactionHistory(Customer customer, int amount,
            TransactionType transactionType = TransactionType.Authorize)
        {
            customer.TransactionHistories ??= new List<TransactionHistory>();
            customer.TransactionHistories.Add(new TransactionHistory
            {
                Amount = amount,
                Type = transactionType
            });
        }

        private void SetupRepositoryMethod(Customer customer)
        {
            mockRepository.Setup(m => m.GetByIdWithIncludeAsync<Customer>(It.IsAny<Guid>(), It.IsAny<string[]>()))
                .ReturnsAsync(customer);
        }

        [SetUp]
        public void SetUp()
        {
            mockRepository = new Mock<IRepository>();
            systemUnderTest = new GatewayService(mockRepository.Object);
        }

        [Test]
        public async Task AuthorizeCustomer_ShouldReturnUniqueId()
        {
            var customerDto = new CustomerDto
            {
                Amount = 10,
                CardNumber = "6331101999990016",
                Currency = "gbp",
                Cvv = 123,
                ExpiryMonth = DateTime.Now.Month.ToString(),
                ExpiryYear = DateTime.Now.Year.ToString()
            };
            var transactionId = Guid.NewGuid();

            mockRepository.Setup(m => m.AddAsync(It.IsAny<Customer>()))
                .Callback((Customer customerTransaction) => { customerTransaction.Id = transactionId; });

            var result = await systemUnderTest.AuthorizeCustomer(customerDto);

            result.ShouldNotBeNull();
            result.TransactionId.ShouldBe(transactionId);
            result.TransactionId.ShouldBeAssignableTo<Guid>();
        }

        [Test]
        public void AuthorizeCustomer_ShouldThrowValidationException_WhenBlacklistedCardIsSent()
        {
            var customerDto = new CustomerDto
            {
                CardNumber = "40000000 0000 0119"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await systemUnderTest.AuthorizeCustomer(customerDto));
        }

        [Test]
        public void AuthorizeCustomer_ShouldThrowValidationException_WhenInvalidCreditCardNumberIsProvided()
        {
            var customerDto = new CustomerDto
            {
                CardNumber = "1332478327"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await systemUnderTest.AuthorizeCustomer(customerDto));
        }

        [Test]
        public async Task Cancel_ShouldReturnOriginalAmount()
        {
            var transactionDto = new TransactionDto
            {
                TransactionId = Guid.NewGuid()
            };

            var customer = GetCustomer(15);
            AddTransactionHistory(customer, 15);
            AddTransactionHistory(customer, 10, TransactionType.Capture);
            AddTransactionHistory(customer, 5, TransactionType.Refund);
            SetupRepositoryMethod(customer);

            var result = await systemUnderTest.Cancel(transactionDto);

            result.Amount.ShouldBe(15);
        }

        [Test]
        public async Task Capture_ShouldReturnRemainingAmount()
        {
            var transactionDto = new TransactionDto
            {
                Amount = 8,
                TransactionId = Guid.NewGuid()
            };

            var customer = GetCustomer(15, Status.CanCapture);
            AddTransactionHistory(customer, 15);
            SetupRepositoryMethod(customer);

            var result = await systemUnderTest.Capture(transactionDto);

            result.Amount.ShouldBe(7);
        }

        [Test]
        public void Capture_ShouldThrowException_WhenCaptureAmountIsGreaterThanAuthorizedAmount()
        {
            var transactionDto = new TransactionDto
            {
                Amount = 100,
                TransactionId = Guid.NewGuid()
            };
            var customer = GetCustomer(15, Status.Refunded);
            AddTransactionHistory(customer, 10);
            SetupRepositoryMethod(customer);

            Assert.ThrowsAsync<ValidationException>(async () => await systemUnderTest.Capture(transactionDto));
        }

        [Test]
        public void Capture_ShouldThrowException_WhenCapturingFullyRefundedCustomer()
        {
            var transactionDto = new TransactionDto
            {
                Amount = 10,
                TransactionId = Guid.NewGuid()
            };
            var customer = GetCustomer(15, Status.Refunded);
            AddTransactionHistory(customer, 10);
            AddTransactionHistory(customer, 10, TransactionType.Refund);
            SetupRepositoryMethod(customer);

            Assert.ThrowsAsync<ValidationException>(async () => await systemUnderTest.Capture(transactionDto));
        }

        [Test]
        public async Task Refund_ShouldReturnRemainingAmount()
        {
            var transactionDto = new TransactionDto
            {
                Amount = 3,
                TransactionId = Guid.NewGuid()
            };

            var customer = GetCustomer(15);
            AddTransactionHistory(customer, 15);
            AddTransactionHistory(customer, 7, TransactionType.Capture);
            SetupRepositoryMethod(customer);

            var result = await systemUnderTest.Refund(transactionDto);

            result.Amount.ShouldBe(11);
        }

        [Test]
        public async Task Refund_ShouldReturnRemainingAmount_WhenFullyRefunded()
        {
            var transactionDto = new TransactionDto
            {
                Amount = 5,
                TransactionId = Guid.NewGuid()
            };

            const int originalAmount = 15;
            var customer = GetCustomer(originalAmount);
            AddTransactionHistory(customer, originalAmount);
            AddTransactionHistory(customer, 10, TransactionType.Capture);
            AddTransactionHistory(customer, 5, TransactionType.Refund);
            SetupRepositoryMethod(customer);

            var result = await systemUnderTest.Refund(transactionDto);

            result.Amount.ShouldBe(originalAmount);
        }

        [Test]
        public void Refund_ShouldThrowException_WhenFullyRefundedCustomer()
        {
            var transactionDto = new TransactionDto
            {
                Amount = 10,
                TransactionId = Guid.NewGuid()
            };
            var customer = GetCustomer(15, Status.Refunded);
            AddTransactionHistory(customer, 10);
            AddTransactionHistory(customer, 10, TransactionType.Refund);
            SetupRepositoryMethod(customer);

            Assert.ThrowsAsync<ValidationException>(async () => await systemUnderTest.Capture(transactionDto));
        }
    }
}