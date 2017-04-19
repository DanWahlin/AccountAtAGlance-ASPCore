using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AccountAtAGlance.Model;
using AccountAtAGlance.Repository.Helpers;
using System.Threading.Tasks;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Repository
{
    public class SecurityRepository : RepositoryBase<AccountAtAGlanceContext>, ISecurityRepository
    {
        //Some random symbols to use in order to get data into the database
        private readonly string[] _StockSymbols = {"AMZN", "BAC", "C", "DIS", "EMC", "FDX", "GE", "H", "INTC", "JPM", "K",
                                                   "LLY", "MSFT", "NKE", "ORCL", "PG", "Q", "RBS", "S", "T", "UL", "V", "WMT",
                                                   "XRX", "YHOO", "ZION", "AAPL", "IBM", "NOK", "CSCO", "FCX", "MTH", "SPF",
                                                   "CRM", "CAT", "LMT", "GD", "XOM", "CVX", "SLB", "BA", "F", "X", "AA",
                                                   "NOC", "RTN","FMAGX", "FDGFX", "FCNTX", "GOOG", "ITRGX", "EBAY", "AOL", "BIDU" };
        IStockEngine _StockEngine;

        public SecurityRepository(IStockEngine stockEngine,
                                  AccountAtAGlanceContext context) : base(context)
        {
            _StockEngine = stockEngine;
        }

        public async Task<Security> GetSecurityAsync(string symbol)
        {
            var stock = await DataContext.Stocks.SingleOrDefaultAsync(s => s.Symbol == symbol.ToUpper());
            if (stock != null)
            {
                stock.DataPoints = new DataSimulator().GetDataPoints(stock.Last);
                return stock;
            }

            var mutual = await DataContext.MutualFunds.SingleOrDefaultAsync(s => s.Symbol == symbol);
            return mutual;
        }

        public async Task<List<TickerQuote>> GetSecurityTickerQuotesAsync()
        {
            return await DataContext.Stocks.Select(s =>
                new TickerQuote
                {
                    Symbol = s.Symbol,
                    Change = s.Change,
                    Last = s.Last
                }).OrderBy(tq => tq.Symbol).ToListAsync();
        }

        public async Task<OperationStatus> UpdateSecuritiesAsync()
        {
            var opStatus = new OperationStatus { Status = true };

            var securities = await DataContext.Stocks.ToArrayAsync(); //Get existing securities
            var updatedSecurities = await _StockEngine.GetSecurityQuotesAsync(securities.Select(s => s.Symbol).ToArray());
            //Return if updatedSecurities is null
            if (updatedSecurities == null) return new OperationStatus { Status = false };

            foreach (var security in securities)
            {
                //Grab updated version of security
                var updatedSecurity = updatedSecurities.Single(s => s.Symbol == security.Symbol);
                security.Change = updatedSecurity.Change;
                security.Last = updatedSecurity.Last;
                security.PercentChange = updatedSecurity.PercentChange;
                security.RetrievalDateTime = updatedSecurity.RetrievalDateTime;
                security.Shares = updatedSecurity.Shares;
                DataContext.Entry(security).State = EntityState.Modified;
            }

            //Insert records
            try
            {
                await DataContext.SaveChangesAsync();
            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException("Error updating security quote.", exp);
            }
            return opStatus;
        }

        public async Task<OperationStatus> InsertSecurityDataAsync()
        {
            var securities = await _StockEngine.GetSecurityQuotesAsync(_StockSymbols);
            var exchanges = securities.OfType<Stock>()
                            .Select(s => s.Exchange.Title).Distinct();

            if (securities != null && securities.Count > 0)
            {
                var opStatus = await DeleteSecurityRecordsAsync(DataContext);
                if (!opStatus.Status) return opStatus;

                opStatus = await InsertExchangesAsync(exchanges, DataContext);
                if (!opStatus.Status) return opStatus;

                opStatus = await InsertSecuritiesAsync(securities, DataContext);
                if (!opStatus.Status) return opStatus;
            }
            return new OperationStatus { Status = true };
        }

        private static async Task<OperationStatus> InsertSecuritiesAsync(IEnumerable<Security> securities, AccountAtAGlanceContext context)
        {
            foreach (var security in securities)
            {
                //Update stock's exchange ID so we don't get dups
                if (security is Stock)
                {
                    var stock = (Stock)security;
                    stock.Exchange = await context.Exchanges.FirstAsync(e => e.Title == stock.Exchange.Title);
                    context.Stocks.Add(stock);
                }
                if (security is MutualFund)
                {
                    var mutualFund = (MutualFund)security;
                    mutualFund.MorningStarRating = 4;
                    context.MutualFunds.Add(mutualFund);
                }
            }

            //Insert records
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException("Error updating security quote.", exp);
            }
            return new OperationStatus { Status = true };
        }

        private async Task<OperationStatus> InsertExchangesAsync(IEnumerable<string> exchanges, AccountAtAGlanceContext context)
        {
            //Insert Exchanges
            foreach (var exchange in exchanges)
            {
                context.Exchanges.Add(new Exchange { Title = exchange });
            }
            try
            {
                await context.SaveChangesAsync(); //Save exchanges so we can get their IDs
            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException("Error updating security exchange.", exp);
            }
            return new OperationStatus { Status = true };
        }

        private async Task<OperationStatus> DeleteSecurityRecordsAsync(AccountAtAGlanceContext context)
        {
            try
            {
                return await context.DeleteSecuritiesAndExchanges();
            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException("Error deleting security/exchange data.", exp);
            }
        }
    }
}
