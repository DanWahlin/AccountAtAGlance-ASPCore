using AccountAtAGlance.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountAtAGlance.Repository.Helpers
{
    public interface IStockEngine
    {
        Task<List<MarketIndex>> GetMarketQuotesAsync(params string[] symbols);
        Task<List<Security>> GetSecurityQuotesAsync(params string[] symbols);
    }
}
