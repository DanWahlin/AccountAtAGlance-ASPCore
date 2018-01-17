using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using AccountAtAGlance.Model;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace AccountAtAGlance.Repository.Helpers
{
    /// <summary>
    /// Used to get some fresh stock quote data into the DB
    /// </summary>
    public class StockEngine : IStockEngine
    {       
        public async Task<List<Security>> GetSecurityQuotesAsync(params string[] symbols)
        {
            XDocument doc = await CreateXDocumentAsync("https://raw.githubusercontent.com/DanWahlin/AccountAtAGlance-ASPCore/master/src/AccountAtAGlance/XML/financequotes.xml");
            if (symbols.Length > 0) FilterSymbols(doc, symbols);
            return ParseSecurities(doc);
        }

        public async Task<List<MarketIndex>> GetMarketQuotesAsync(params string[] symbols)
        {
            XDocument doc = await CreateXDocumentAsync("https://raw.githubusercontent.com/DanWahlin/AccountAtAGlance-ASPCore/master/src/AccountAtAGlance/XML/marketquotes.xml");
            if (symbols.Length > 0) FilterSymbols(doc, symbols);
            return ParseMarketIndexes(doc);
        }

        private async Task<XDocument> CreateXDocumentAsync(string baseUrl)
        {
            try
            {
                var httpClient = new HttpClient();
                string xml = await httpClient.GetStringAsync(baseUrl);
                XDocument doc = XDocument.Load(new StringReader(xml));
                return doc;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return null;
        }

        private void FilterSymbols(XDocument doc, params string[] symbols)
        {
            IEnumerable<XElement> quotes = doc.Root.Descendants("finance");
            var matchedQuotes = quotes.Where(q => symbols.Contains(GetAttributeData(q, "symbol"), StringComparer.OrdinalIgnoreCase)).ToList();
            doc.Root.RemoveAll();
            foreach (var quote in matchedQuotes)
            {
                doc.Root.Add(quote);
            }
        }

        private List<Security> ParseSecurities(XDocument doc)
        {
            if (doc == null) return null;
            List<Security> securities = new List<Security>();

            IEnumerable<XElement> quotes = doc.Root.Descendants("finance");

            foreach (var quote in quotes)
            {
                var symbol = GetAttributeData(quote, "symbol");
                var exchange = GetAttributeData(quote, "exchange");
                var last = GetDecimal(quote, "last");
                var change = GetDecimal(quote, "change");
                var percentChange = GetDecimal(quote, "perc_change");
                var company = GetAttributeData(quote, "company");

                if (exchange.ToUpper() == "MUTF") //Handle mutual fund
                {
                    var mf = new MutualFund();
                    mf.Symbol = symbol;
                    mf.Last = last;
                    mf.Change = change;
                    mf.PercentChange = percentChange;
                    mf.RetrievalDateTime = DateTime.Now;
                    mf.Company = company;
                    securities.Add(mf);
                }
                else //Handle stock
                {
                    var stock = new Stock();
                    stock.Symbol = symbol;
                    stock.Last = last;
                    stock.Change = change;
                    stock.PercentChange = percentChange;
                    stock.RetrievalDateTime = DateTime.Now;
                    stock.Company = company;
                    stock.Exchange = new Exchange { Title = exchange };
                    stock.DayHigh = GetDecimal(quote, "high");
                    stock.DayLow = GetDecimal(quote, "low");
                    stock.Volume = GetDecimal(quote, "volume");
                    stock.AverageVolume = GetDecimal(quote, "avg_volume");
                    stock.MarketCap = GetDecimal(quote, "market_cap");
                    stock.Open = GetDecimal(quote, "open");
                    securities.Add(stock);
                }
            }
            return securities;
        }

        private List<MarketIndex> ParseMarketIndexes(XDocument doc)
        {
            if (doc == null) return null;
            List<MarketIndex> marketIndexes = new List<MarketIndex>();

            IEnumerable<XElement> quotes = doc.Root.Descendants("finance");

            foreach (var quote in quotes)
            {
                var index = new MarketIndex();
                index.Symbol = GetAttributeData(quote, "symbol"); ;
                index.Last = GetDecimal(quote, "last");
                index.Change = GetDecimal(quote, "change");
                index.PercentChange = GetDecimal(quote, "perc_change");
                index.RetrievalDateTime = DateTime.Now;
                index.Title = GetAttributeData(quote, "company");
                index.DayHigh = GetDecimal(quote, "high");
                index.DayLow = GetDecimal(quote, "low");
                index.Volume = GetDecimal(quote, "volume");
                index.Open = GetDecimal(quote, "open");
                marketIndexes.Add(index);
            }
            return marketIndexes;
        }

        private string GetAttributeData(XElement quote, string elemName)
        {
            return quote.Element(elemName).Attribute("data").Value;
        }

        private decimal GetDecimal(XElement quote, string elemName)
        {
            var input = GetAttributeData(quote, elemName);
            if (input == null) return 0.00M;

            decimal value;

            if (Decimal.TryParse(input, out value)) return value;
            return 0.00M;
        }

        private long GetLong(XElement quote, string elemName)
        {
            var input = GetAttributeData(quote, elemName);
            if (input == null) return 0L;

            long value;

            if (long.TryParse(input, out value)) return value;
            return 0L;
        }

        private DateTime GetDateTime(XElement quote, string elemName)
        {
            var input = GetAttributeData(quote, elemName);
            if (input == null) return DateTime.Now; ;

            DateTime value;

            if (DateTime.TryParse(input, out value)) return value;
            return DateTime.Now;
        }

        public StockEngine()
        {
            //Added to work around some companies blocking DropBox links
            //string port = System.Web.HttpContext.Current.Request.Url.Port.ToString();
            //marketsUrl = "http://localhost:" + port + "/XML/marketquotes.xml";
            //financeUrl = "http://localhost:" + port + "/XML/financequotes.xml";
        }

    }
}