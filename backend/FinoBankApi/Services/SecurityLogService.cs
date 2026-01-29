using FinoBankApi.Data;
using FinoBankApi.Models;

namespace FinoBankApi.Services
{
    public class SecurityLogService : ISecurityLogService
    {
        private readonly BankingDbContext _context;
        public SecurityLogService(BankingDbContext context) { _context = context; }

        public async Task LogAsync(int? userId, string action, string desc, string ip)
        {
            _context.SecurityLogs.Add(new SecurityLog 
            { 
                UserId = userId, Action = action, Description = desc, IpAddress = ip 
            });
            await _context.SaveChangesAsync();
        }
    }
}