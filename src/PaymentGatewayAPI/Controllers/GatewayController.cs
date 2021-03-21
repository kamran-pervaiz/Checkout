using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Dto;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private readonly IGatewayService gatewayService;
        private readonly ILogger<GatewayController> logger;

        public GatewayController(IGatewayService gatewayService, ILogger<GatewayController> logger = null)
        {
            this.gatewayService = gatewayService;
            this.logger = logger ?? new ConsoleLogger<GatewayController>();
        }

        [HttpPost]
        [Route("authorize")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorizeResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authorize(
            [FromBody] CustomerDto customerDetail)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                {"customer card", customerDetail}
            }))
            {
                var result = await gatewayService.AuthorizeCustomer(customerDetail);
                logger.LogInformation("authorized the customer successfully");
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("capture")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Capture([FromBody] TransactionDto transactionDto)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                {"transactionId", transactionDto.TransactionId}
            }))
            {
                var result = await gatewayService.Capture(transactionDto);
                logger.LogInformation("Captured the amount");
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("refund")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refund([FromBody] TransactionDto transactionDto)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                {"transactionId", transactionDto.TransactionId}
            }))
            {
                var result = await gatewayService.Refund(transactionDto);
                logger.LogInformation("Refunded the amount");
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("void")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel([FromBody] TransactionDto transactionDto)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                {"transactionId", transactionDto.TransactionId}
            }))
            {
                var result = await gatewayService.Cancel(transactionDto);
                logger.LogInformation("Cancelled the whole transaction");
                return Ok(result);
            }
        }
    }
}