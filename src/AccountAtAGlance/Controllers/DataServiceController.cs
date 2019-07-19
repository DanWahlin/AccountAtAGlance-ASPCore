using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AccountAtAGlance.Repository;
using AccountAtAGlance.Model;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Controllers
{
    [Route("api/[controller]")]
    public class DataServiceController : ControllerBase
    {
        IAccountRepository _AccountRepository;
        ISecurityRepository _SecurityRepository;
        IMarketsAndNewsRepository _MarketRepository;

        public DataServiceController(IAccountRepository acctRepo,
          ISecurityRepository secRepo, IMarketsAndNewsRepository marketRepo)
        {
            _AccountRepository = acctRepo;
            _SecurityRepository = secRepo;
            _MarketRepository = marketRepo;
        }


        [HttpGet("account/{acctNumber}")]
        public async Task<IActionResult> Account(string acctNumber)
        {
            var acct = await _AccountRepository.GetAccountAsync(acctNumber);
            if (acct == null)
            {
                return NotFound();
            }
            return Ok(acct);
        }

        [HttpGet("quote/{symbol}")]
        public async Task<IActionResult> Quote(string symbol)
        {
            var security = await _SecurityRepository.GetSecurityAsync(symbol);
            if (security == null)
            {
                return NotFound();
            }
            return Ok(security);
        }

        [HttpGet("marketIndices")]
        public async Task<IActionResult> MarketIndices()
        {
            var marketQuotes = await _MarketRepository.GetMarketsAsync();
            if (marketQuotes == null)
            {
                return NotFound();
            }
            return Ok(marketQuotes);
        }

        [HttpGet("tickerQuotes")]
        public async Task<IActionResult> TickerQuotes()
        {
            var marketQuotes = await _MarketRepository.GetMarketTickerQuotesAsync();
            var securityQuotes = await _SecurityRepository.GetSecurityTickerQuotesAsync();
            marketQuotes.AddRange(securityQuotes);
            var news = await _MarketRepository.GetMarketNewsAsync();

            if (marketQuotes == null && news == null)
            {
                return NotFound();
            }

            return Ok(new MarketsAndNews { Markets = marketQuotes, News = news });
        }

        //[HttpPost("account")]
        //public async Task<IActionResult> Account(BrokerageAccount acct)
        //{
        //    var opStatus = await _AccountRepository.InsertAccountAsync(acct);

        //    if (!opStatus.Status)
        //    {
        //        return HttpBadRequest(opStatus);
        //    }

        //    //Generate success response
        //    return Created(Request.Path.Value + opStatus.OperationID.ToString(), opStatus);
        //}
    }
}
