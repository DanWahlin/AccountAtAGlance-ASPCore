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

    public class AccountRepository : RepositoryBase<AccountAtAGlanceContext>, IAccountRepository
    {
        IStockEngine _StockEngine;
        ISecurityRepository _SecurityRepository;

        public AccountRepository(IStockEngine stockEngine,
            ISecurityRepository securityRepository,
            AccountAtAGlanceContext context) : base(context)
        {
            _StockEngine = stockEngine;
            _SecurityRepository = securityRepository;
        }

        public async Task<Customer> GetCustomerAsync(string custId)
        {
            return await DataContext.Customers
                .Include(c => c.BrokerageAccounts)
                .SingleOrDefaultAsync(c => c.CustomerCode == custId);
        }

        public async Task<OperationStatus> InsertAccountAsync(BrokerageAccount acct)
        {
            //simulate insert operation success
            return await Task.Run(() => new OperationStatus { Status = true });
        }

        public async Task<BrokerageAccount> GetAccountAsync(string acctNumber)
        {
            if (String.IsNullOrEmpty(acctNumber)) return null;
            return await UpdateAccountAsync(acctNumber);
        }

        private async Task<BrokerageAccount> UpdateAccountAsync(string acctNumber)
        {
            //Force update of security values
            await _SecurityRepository.UpdateSecuritiesAsync();

            var acct = await DataContext.BrokerageAccounts
                .Include(ba => ba.Positions)
                //.Include(ba => ba.Positions.Select(pos => pos.Security))
                .SingleOrDefaultAsync(ba => ba.AccountNumber == acctNumber);

            //Hack: Had to break out positions/securities query due to EF Core limitation
            //https://github.com/aspnet/EntityFramework/issues/3163
            var securityIds = acct.Positions.Select(p => p.SecurityId).ToList();
            var securities = await DataContext.Stocks
                .Where(s => securityIds.Contains(s.Id))
                .ToListAsync();

            foreach (var pos in acct.Positions)
            {
                pos.Security = securities.Single(s => s.Id == pos.SecurityId);
            }

            if (acct != null && acct.Positions != null)
            {
                acct.Positions = acct.Positions.OrderBy(p => p.Total).ToList();

                //Get account position securities
                //var securities = acct.Positions.Select(p => p.Security).Distinct().ToList();

                //var positions = acct.Positions;
                //foreach (var pos in positions)
                //{
                //    pos.Total = pos.Shares * pos.Security.Last;
                //    DataContext.Entry(pos).State = EntityState.Modified;
                //}
                acct.PositionsTotal = acct.Positions.Sum(p => p.Total);
                acct.Total = acct.PositionsTotal + acct.CashTotal;

                DataContext.Entry(acct).State = EntityState.Modified;

                await DataContext.SaveChangesAsync();
            }

            return acct;
        }

        public async Task<BrokerageAccount> GetAccountAsync(int id)
        {
            return await DataContext.BrokerageAccounts
                .Include(ba => ba.Orders)
                .Include(ba => ba.Positions)
                .SingleOrDefaultAsync(ba => ba.Id == id);
        }

        #region Seeding

        public async Task<OperationStatus> CreateCustomerAsync()
        {
            var opStatus = new OperationStatus { Status = true };
            try
            {
                await DataContext.DeleteAccounts();

                if (opStatus.Status)
                {
                    var cust = new Customer
                    {
                        FirstName = "Marcus",
                        LastName = "Hightower",
                        Address = "1234 Anywhere St.",
                        City = "Phoenix",
                        State = "AZ",
                        Zip = 85229,
                        CustomerCode = "C15643"
                    };
                    DataContext.Customers.Add(cust);
                    var accts = CreateBrokerageAccounts(cust);
                    foreach (var acct in accts)
                    {
                        cust.BrokerageAccounts.Add(acct);
                    }
                    await DataContext.SaveChangesAsync();
                }
            }
            catch (Exception exp)
            {
                opStatus = OperationStatus.CreateFromException("Error updating security exchange: " + exp.Message, exp);
            }

            return opStatus;
        }

        private List<BrokerageAccount> CreateBrokerageAccounts(Customer cust)
        {
            List<BrokerageAccount> accts = new List<BrokerageAccount>();
            string[] accountTitles = { "IRA", "Joint Brokerage", "Brokerage Account" };
            for (int i = 0; i < accountTitles.Length; i++)
            {
                var acct = new BrokerageAccount
                {
                    AccountNumber = "Z48573988" + i.ToString(),
                    AccountTitle = accountTitles[i],
                    IsRetirement = (i == 0) ? true : false,
                    CashTotal = (i + 1) * 5000,
                    CustomerId = cust.Id,
                    Customer = cust
                };

                //FillAccountSecurities(securities, acct, i);

                acct.PositionsTotal = acct.Positions.Sum(p => p.Total);
                acct.Total = acct.PositionsTotal + acct.CashTotal;
                acct.MarginBalance = (acct.IsRetirement) ? 0.00M : Math.Round(acct.Total / 3, 2);
                accts.Add(acct);
            }

            return accts;
        }

        //Had to break this out into a separate DB call due to current state of EF7
        public async Task<OperationStatus> CreateAccountPositionsAsync()
        {
            var opStatus = new OperationStatus { Status = true };
            try
            {
                var securities = await DataContext.Stocks.ToListAsync();
                var accounts = await DataContext.BrokerageAccounts.ToListAsync();

                for (int i = 0; i < accounts.Count; i++)
                {
                    var account = accounts[i];
                    var rdm = new Random((int)DateTime.Now.Ticks + i);
                    for (int index = 0; index < 10; index++)
                    {
                        int pos = rdm.Next(securities.Count - 1);
                        var stock = securities[pos];
                        if (!account.Positions.Any(p => p.Security.Symbol == stock.Symbol))
                        {
                            var multiplier = (pos == 0) ? 1 : pos;
                            var shares = multiplier * 100;
                            var total = shares * stock.Last;
                            var position = new Position
                            {
                                SecurityId = stock.Id,
                                Security = stock,
                                Shares = shares,
                                Total = total
                            };
                            account.Positions.Add(position);
                        }
                    }
                }

                await DataContext.SaveChangesAsync();
            }
            catch (Exception exp)
            {
                opStatus = OperationStatus.CreateFromException("Error updating security exchange: " + exp.Message, exp);
            }
            return opStatus;
        }

        #endregion
    }
}
