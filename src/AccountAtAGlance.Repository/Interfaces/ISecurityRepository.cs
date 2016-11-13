using System.Collections.Generic;
using AccountAtAGlance.Model;
using System.Threading.Tasks;

namespace AccountAtAGlance.Repository.Interfaces
{
    public interface ISecurityRepository
    {
        Task<Security> GetSecurityAsync(string symbol);
        Task<List<TickerQuote>> GetSecurityTickerQuotesAsync();
        Task<OperationStatus> UpdateSecuritiesAsync();
        Task<OperationStatus> InsertSecurityDataAsync();
    }
}