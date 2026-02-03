using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinoBankApi.Data;
using FinoBankApi.Models;
using FinoBankApi.DTOs;

namespace FinoBankApi.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly BankingDbContext _context;
        public ContactsController(BankingDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok(await _context.Contacts.Where(c => c.UserId == userId).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            var normalizedAccount = dto.AccountNumber.Replace(" ", "").ToUpper(); // Używamy dto
            if (!normalizedAccount.StartsWith("PL"))
            {
                normalizedAccount = "PL" + normalizedAccount;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedAccount, @"^PL\d{26}$"))
            {
                return BadRequest(new { message = "Numer konta musi składać się z PL i 26 cyfr." });
            }
            
            var contact = new Contact
            {
                UserId = userId, 
                Name = dto.Name,
                AccountNumber = normalizedAccount
            };
            
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            
            return Ok(contact);
        }
    }
}