using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinoBankApi.DTOs;
using FinoBankApi.Services;
using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly PdfService _pdfService;

        // Wstrzykujemy PdfService przez konstruktor
        public TransactionsController(ITransactionService transactionService, PdfService pdfService)
        {
            _transactionService = transactionService;
            _pdfService = pdfService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto)
        {
            var transaction = await _transactionService.CreateTransaction(createTransactionDto, GetUserId());
            return Ok(transaction);
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetAccountTransactions(int accountId)
        {
            var transactions = await _transactionService.GetAccountTransactions(accountId, GetUserId());
            return Ok(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTransactions()
        {
            var transactions = await _transactionService.GetUserTransactions(GetUserId());
            return Ok(transactions);
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var t = await _transactionService.GetTransactionById(id, GetUserId());

            if (t == null) return NotFound(new { message = "Nie znaleziono transakcji lub brak dostępu." });

            var fileBytes = _pdfService.GenerateTransactionPdf(
                t.Title, 
                t.Amount, 
                t.CreatedAt.ToString("dd.MM.yyyy HH:mm"), 
                t.FromAccountNumber, 
                t.ToAccountNumber
            );

            return File(fileBytes, "application/pdf", $"potwierdzenie_{id}.pdf");
        }
    }
}