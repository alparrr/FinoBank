using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinoBankApi.DTOs;
using FinoBankApi.Services;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/cards")]
    [Authorize]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        [HttpGet]
        public async Task<IActionResult> GetMyCards()
        {
            var cards = await _cardService.GetUserCards(GetUserId());
            return Ok(cards);
        }

        [HttpPost("create/{accountId}")]
        public async Task<IActionResult> CreateCard(int accountId)
        {
            await _cardService.CreateCardForAccount(accountId, GetUserId());
            return Ok(new { message = "Card ordered successfully" });
        }

        [HttpPost("{cardId}/limits")]
        public async Task<IActionResult> UpdateLimits(int cardId, [FromBody] UpdateLimitsDto dto)
        {
            await _cardService.ChangeLimits(cardId, GetUserId(), dto.DailyLimit, dto.MonthlyLimit);
            return Ok(new { message = "Limits updated" });
        }
        
        [HttpPost("{cardId}/block")]
        public async Task<IActionResult> Block(int cardId)
        {
            await _cardService.BlockCard(cardId, GetUserId());
            return Ok(new { message = "Card blocked" });
        }
    }
}