using FinoBankApi.Data;
using FinoBankApi.Models;

namespace FinoBankApi.Services
{
    public interface ISecurityLogService
    {
        Task LogAsync(int? userId, string action, string desc, string ip);
    }
}