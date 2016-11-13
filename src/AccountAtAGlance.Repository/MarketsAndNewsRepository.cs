using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AccountAtAGlance.Model;
using AccountAtAGlance.Repository.Helpers;
using System.Threading.Tasks;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Repository
{
    public class MarketsAndNewsRepository : RepositoryBase<AccountAtAGlanceContext>, IMarketsAndNewsRepository
    {
        string[] _MarketIndexSymbols = new string[] { ".DJI", ".IXIC", ".INX" };
        IStockEngine _StockEngine;

        public MarketsAndNewsRepository(IStockEngine stockEngine, 
                                        AccountAtAGlanceContext context) : base(context)
        {
            _StockEngine = stockEngine;
        }

        public async Task<MarketQuotes> GetMarketsAsync()
        {
            var markets = await _StockEngine.GetMarketQuotesAsync(_MarketIndexSymbols);
            return BuildMarketQuotes(markets);
        }

        private MarketQuotes BuildMarketQuotes(IEnumerable<MarketIndex> markets)
        {
            var rdm = new Random((int)DateTime.Now.Ticks);
            var mult = ((decimal)rdm.NextDouble() / 100) + 1;
            MarketQuotes marketQuotes;
            if (markets != null)
            {
                marketQuotes = new MarketQuotes
                {
                    DOW = markets.Single(m => m.Symbol == ".DJI"),
                    NASDAQ = markets.Single(m => m.Symbol == ".IXIC"),
                    SP500 = markets.Single(m => m.Symbol == ".INX")
                };
            }
            else
            {
                marketQuotes = new MarketQuotes
                {
                    DOW = new MarketIndex { Change = 0, DayHigh = 0, DayLow = 0, Last = 12000 },
                    NASDAQ = new MarketIndex { Change = 0, DayHigh = 0, DayLow = 0, Last = 2700 },
                    SP500 = new MarketIndex { Change = 0, DayHigh = 0, DayLow = 0, Last = 800 }
                };
            }
            marketQuotes.DOW.Last = marketQuotes.DOW.Last * mult;
            marketQuotes.DOW.Change = Math.Round(marketQuotes.DOW.Change * mult, 2);
            marketQuotes.DOW.PercentChange = Math.Round(marketQuotes.DOW.PercentChange * mult, 2);

            marketQuotes.NASDAQ.Last = marketQuotes.NASDAQ.Last * mult;
            marketQuotes.NASDAQ.Change = Math.Round(marketQuotes.NASDAQ.Change * mult, 2);
            marketQuotes.NASDAQ.PercentChange = Math.Round(marketQuotes.NASDAQ.PercentChange * mult, 2);

            marketQuotes.SP500.Last = marketQuotes.SP500.Last * mult;
            marketQuotes.SP500.Change = Math.Round(marketQuotes.SP500.Change * mult, 2);
            marketQuotes.SP500.PercentChange = Math.Round(marketQuotes.SP500.PercentChange * mult, 2);

            var sim = new DataSimulator();
            marketQuotes.DOWDataPoints = sim.GetDataPoints(marketQuotes.DOW.Last);
            marketQuotes.NASDAQDataPoints = sim.GetDataPoints(marketQuotes.NASDAQ.Last);
            marketQuotes.SP500DataPoints = sim.GetDataPoints(marketQuotes.SP500.Last);
            return marketQuotes;
        }

        public async Task<List<string>> GetMarketNewsAsync()
        {
            return await Task.Run(() =>
                new List<string> { "TOP Oil Market News: Crude Gains; Heating Oil to Pass Gasoline", "Canada Dollar Steadies As Oil Prices Look Higher", "FOMC Outlook: Non-Event for Markets", "Gold Prices Bounce", "Covidien CEO To Retire; Head Of Medical Devices Named Successor" }
            );
        }

        public async Task<OperationStatus> InsertMarketDataAsync()
        {
            var marketIndexes = await _StockEngine.GetMarketQuotesAsync(_MarketIndexSymbols);

            if (marketIndexes != null && marketIndexes.Count > 0)
            {
                var opStatus = DeleteMarketIndexRecords(DataContext);
                if (!opStatus.Status) return opStatus;

                foreach (var marketIndex in marketIndexes)
                {
                    DataContext.MarketIndexes.Add(marketIndex);
                }

                try
                {
                    await DataContext.SaveChangesAsync();
                }
                catch (Exception exp)
                {
                    return OperationStatus.CreateFromException("Error inserting market index data.", exp);
                }
            }
            return new OperationStatus { Status = true };
        }

        private OperationStatus DeleteMarketIndexRecords(AccountAtAGlanceContext context)
        {
            try
            {
                ExecuteStoreCommand("DELETE FROM MarketIndex;");
            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException("Error deleting market index data.", exp);
            }
            return new OperationStatus { Status = true };
        }

        public async Task<List<TickerQuote>> GetMarketTickerQuotesAsync()
        {
            return await GetList<MarketIndex>().Select(s =>
                new TickerQuote
                {
                    Symbol = s.Symbol,
                    Change = s.Change,
                    Last = s.Last
                }).ToListAsync();
        }
    }
}
