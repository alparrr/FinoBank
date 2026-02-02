using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinoBankApi.Data;
using FinoBankApi.Models;

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
        public async Task<IActionResult> Create([FromBody] Contact contact)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            contact.UserId = userId;
            
            var normalizedAccount = contact.AccountNumber.Replace(" ", "").ToUpper();

            if (!normalizedAccount.StartsWith("PL"))
            {
                normalizedAccount = "PL" + normalizedAccount;
            }

            contact.AccountNumber = normalizedAccount;

            if (!System.Text.RegularExpressions.Regex.IsMatch(contact.AccountNumber, @"^PL\d{26}$"))
            {
                return BadRequest("Format must be PL followed by 26 digits");
            }
            
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return Ok(contact);
        }
    }
}