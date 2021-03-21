using Application.Common.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Application.Dto;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using WebAPI.Controllers;

namespace WebApi.UnitTests.Controllers
{
    [TestFixture]
    public class GatewayControllerTests
    {
        private Mock<IGatewayService> mockGatewayService;
        private GatewayController systemUnderTest;

        [SetUp]
        public void SetUp()
        {
            mockGatewayService = new Mock<IGatewayService>();
            systemUnderTest = new GatewayController(mockGatewayService.Object);
        }

        [Test]
        public async Task Authorize_ShouldReturnUniqueId()
        {
            // Arrange
            var creditDetail = new CustomerDto
            {
                Amount = 12,
                CardNumber = "1234567898765432",
                Currency = "gbp",
                Cvv = 123,
                ExpiryMonth = DateTime.Now.Month.ToString(),
                ExpiryYear = DateTime.Now.Year.ToString()
            };
            mockGatewayService.Setup(m => m.AuthorizeCustomer(creditDetail))
                .ReturnsAsync(new AuthorizeResponse(creditDetail.Amount, creditDetail.Currency, Guid.NewGuid()));

            // Act
            var result = await systemUnderTest.Authorize(creditDetail);
            var okResult = result as OkObjectResult;

            // Assert
            mockGatewayService.Verify(m => m.AuthorizeCustomer(creditDetail), Times.Once);
            AssertOkResult(okResult);
        }

        [Test]
        public async Task Capture_ShouldReturnRemainingAmount()
        {
            var paymentDto = GetPaymentDto();
            mockGatewayService.Setup(m => m.Capture(paymentDto))
                .ReturnsAsync(new PaymentResponse(1, "gbp"));

            var result = await systemUnderTest.Capture(paymentDto);
            var okResult = result as OkObjectResult;

            mockGatewayService.Verify(m => m.Capture(paymentDto), Times.Once);
            AssertOkResult(okResult);
        }

        [Test]
        public async Task Refund_ShouldReturnRemainingAmount()
        {
            var paymentDto = GetPaymentDto();
            mockGatewayService.Setup(m => m.Refund(paymentDto))
                .ReturnsAsync(new PaymentResponse(1, "euro"));

            var result = await systemUnderTest.Refund(paymentDto);
            var okResult = result as OkObjectResult;

            mockGatewayService.Verify(m => m.Refund(paymentDto), Times.Once);
            AssertOkResult(okResult);
        }

        [Test]
        public async Task Cancel_ShouldReturnOriginalAmount()
        {
            var paymentDto = GetPaymentDto();
            mockGatewayService.Setup(m => m.Cancel(paymentDto))
                .ReturnsAsync(new PaymentResponse(1, "dollar"));

            var result = await systemUnderTest.Cancel(paymentDto);
            var okResult = result as OkObjectResult;

            mockGatewayService.Verify(m => m.Cancel(paymentDto), Times.Once);
            AssertOkResult(okResult);
        }

        private static TransactionDto GetPaymentDto()
        {
            return new TransactionDto
            {
                TransactionId = Guid.NewGuid(),
                Amount = 3
            };
        }

        private static void AssertOkResult(OkObjectResult okResult)
        {
            okResult.ShouldNotBeNull();
            okResult.StatusCode.ShouldBe(200);
        }
    }
}
