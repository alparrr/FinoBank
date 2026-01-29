using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinoBankApi.DTOs;
using FinoBankApi.Services;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccounts(userId);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var userId = GetUserId();
            var account = await _accountService.GetAccountById(id, userId);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
        {
            var userId = GetUserId();
            var account = await _accountService.CreateAccount(createAccountDto, userId);
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }

        [HttpGet("{id}/balance")]
        public async Task<IActionResult> GetBalance(int id)
        {
            var userId = GetUserId();
            var balance = await _accountService.GetAccountBalance(id, userId);
            return Ok(new { balance });
        }
    }
}