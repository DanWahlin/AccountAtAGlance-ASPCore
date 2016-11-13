using System.Collections.Generic;
using AccountAtAGlance.Model;
using System.Threading.Tasks;

namespace AccountAtAGlance.Repository.Interfaces
{
    public interface IMarketsAndNewsRepository
    {
        Task<MarketQuotes> GetMarketsAsync();
        Task<List<TickerQuote>> GetMarketTickerQuotesAsync();
        Task<List<string>> GetMarketNewsAsync();
        Task<OperationStatus> InsertMarketDataAsync();
    }
}