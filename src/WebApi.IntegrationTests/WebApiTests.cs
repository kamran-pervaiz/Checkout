using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.Dto;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using WebAPI;

namespace WebApi.IntegrationTests
{
    public class WebApiTests
    {
        private HttpClient client;

        private IServiceProvider services;
        //private TestServer server;

        [OneTimeSetUp]
        public void GivenARequestToTheController()
        {
            var factory = new ApiWebApplicationFactory<Startup>();
            client = factory.CreateClient();

            services = factory.Services;
        }

        [Test]
        public async Task Authorize_ShouldReturnTransactionId()
        {
            var authorizeResponseDto = await AuthorizeCustomerCreditData(10);
            authorizeResponseDto.TransactionId.ShouldBeAssignableTo<Guid>();
        }

        [Test]
        public async Task Capture_ShouldReturnRemainingAmount()
        {
            // Arrange
            var authorizeResponseDto = await AuthorizeCustomerCreditData(15);

            // Act
            var result =
                await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.CaptureEndpoint);

            // Assert
            result.Amount.ShouldBe(12);
            result.Currency.ShouldBe("gbp");
        }

        [Test]
        public async Task Capture_ShouldReturnRemainingAmountWhenCalledMultipleTimes()
        {
            var authorizeResponseDto = await AuthorizeCustomerCreditData(10);
            await RunTransactionOnCustomerBank(1, authorizeResponseDto.TransactionId, Constants.CaptureEndpoint);
            var result =
                await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.CaptureEndpoint);

            result.Amount.ShouldBe(6);
            result.Currency.ShouldBe("gbp");
        }

        [Test]
        public async Task Refund_ShouldReturnRemainingAmount()
        {
            var authorizeResponseDto = await AuthorizeCustomerCreditData(10);
            await RunTransactionOnCustomerBank(6, authorizeResponseDto.TransactionId, Constants.CaptureEndpoint);

            var result =
                await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.RefundEndpoint);

            result.Amount.ShouldBe(7);
            result.Currency.ShouldBe("gbp");
        }

        [Test]
        public async Task Refund_ShouldReturnRemainingAmountWhenFullyRefunded()
        {
            var authorizeResponseDto = await AuthorizeCustomerCreditData(10);
            await RunTransactionOnCustomerBank(6, authorizeResponseDto.TransactionId, Constants.CaptureEndpoint);
            await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.RefundEndpoint);

            var result =
                await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.RefundEndpoint);

            result.Amount.ShouldBe(10);
            result.Currency.ShouldBe("gbp");
        }

        [Test]
        public async Task Void_ShouldCancelTransactionAndReturnOriginalAmount()
        {
            var authorizeResponseDto = await AuthorizeCustomerCreditData(11);
            await RunTransactionOnCustomerBank(6, authorizeResponseDto.TransactionId, Constants.CaptureEndpoint);
            await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.RefundEndpoint);

            var result =
                await RunTransactionOnCustomerBank(3, authorizeResponseDto.TransactionId, Constants.VoidEndpoint);

            result.Amount.ShouldBe(11);
            result.Currency.ShouldBe("gbp");
        }

        private async Task<AuthorizeResponse> AuthorizeCustomerCreditData(int amount)
        {
            var jsonString = GetCreditDataAsJsonString(amount);
            var response = await PostAsync(jsonString, Constants.AuthorizeEndpoint);
            var authorizeResponseDto =
                JsonConvert.DeserializeObject<AuthorizeResponse>(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
            return authorizeResponseDto;
        }

        private async Task<PaymentResponse> RunTransactionOnCustomerBank(int amount, Guid transactionId,
            string endpoint)
        {
            var transactionDto = GetTransactionDtoAsJsonString(amount, transactionId);
            var response = await PostAsync(transactionDto, endpoint);
            var result = JsonConvert.DeserializeObject<PaymentResponse>(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
            return result;
        }

        private async Task<HttpResponseMessage> PostAsync(string jsonString, string endpoint)
        {
            return await client.PostAsync(endpoint, new StringContent(jsonString, Encoding.UTF8, Constants.MediaType));
        }

        private static string GetTransactionDtoAsJsonString(int amount, Guid transactionId)
        {
            var transactionDto = new TransactionDto
            {
                Amount = amount,
                TransactionId = transactionId
            };

            return JsonConvert.SerializeObject(transactionDto);
        }

        private static string GetCreditDataAsJsonString(int amount)
        {
            var customerDto = new CustomerDto
            {
                Amount = amount,
                CardNumber = "6331101999990016",
                Currency = "gbp",
                Cvv = 123,
                ExpiryMonth = DateTime.Now.Month.ToString(),
                ExpiryYear = DateTime.Now.Year.ToString()
            };
            var jsonString = JsonConvert.SerializeObject(customerDto);
            return jsonString;
        }
    }
}