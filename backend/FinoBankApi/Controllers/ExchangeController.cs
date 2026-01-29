using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinoBankApi.DTOs;
using FinoBankApi.Services;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/exchange")]
    [Authorize]
    public class ExchangeController : ControllerBase
    {
        private readonly IExchangeService _exchangeService;

        public ExchangeController(IExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        [HttpGet("rates")]
        public async Task<IActionResult> GetRates()
        {
            await _exchangeService.UpdateRatesFromNbp();
            var rates = await _exchangeService.GetCurrentRates();
            return Ok(rates);
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExchangeMoney([FromBody] ExchangeRequestDto dto)
        {
            await _exchangeService.ExchangeCurrency(GetUserId(), dto);
            return Ok(new { message = "Exchange successful" });
        }
    }
}