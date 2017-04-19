using System;
using Microsoft.EntityFrameworkCore;
using AccountAtAGlance.Model;
using System.Threading.Tasks;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Repository
{
    public class AccountAtAGlanceContext : DbContext, IDisposedTracker
    {
        public AccountAtAGlanceContext()
        {

        }

        public AccountAtAGlanceContext(DbContextOptions<AccountAtAGlanceContext> options)
            : base(options)
        { }

        public DbSet<BrokerageAccount> BrokerageAccounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<MarketIndex> MarketIndexes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<MutualFund> MutualFunds { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public bool IsDisposed { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Ignores
            modelBuilder.Ignore<DataPoint>();
            modelBuilder.Ignore<MarketsAndNews>();
            modelBuilder.Ignore<MarketQuotes>();
            modelBuilder.Ignore<OperationStatus>();
            modelBuilder.Ignore<TickerQuote>();
            modelBuilder.Ignore<Security>();

            // Map these class names to the table names in the DB for Table Per Type (TPT)
            // Currently not supported in EF Core

            //modelBuilder.Entity<Security>().ToTable("Security");
            //modelBuilder.Entity<Stock>().ToTable("Stock");
            //modelBuilder.Entity<MutualFund>().ToTable("MutualFund");

            //Create one to many relationship between Customer/BrokerageAccounts
            //modelBuilder.Entity<Customer>()
            //    .Collection(c => c.BrokerageAccounts)
            //    .InverseReference(ba => ba.Customer)
            //    .ForeignKey(ba => ba.CustomerId);

            //modelBuilder.Entity<BrokerageAccount>()
            //    .Collection(ba => ba.Positions)
            //    .InverseReference(p => p.BrokerageAccount)
            //    .ForeignKey(p => p.BrokerageAccountId);
        }

        public override void Dispose()
        {
            IsDisposed = true;
            base.Dispose();
        }

        public async Task<OperationStatus> DeleteAccounts()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                //await Database.ExecuteSqlCommandAsync("DeleteAccounts");
                await Database.ExecuteSqlCommandAsync(@"
				    DELETE FROM Orders;                                              
				    DELETE FROM BrokerageAccounts;
				    DELETE FROM Customers;	
                ");

            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException(exp.Message, exp);
            }
            return opStatus;
        }

        public async Task<OperationStatus> DeleteSecuritiesAndExchanges()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                //await Database.ExecuteSqlCommandAsync("DeleteSecuritiesAndExchanges");
                await Database.ExecuteSqlCommandAsync(@"
			      DELETE FROM Positions;   
			      DELETE FROM Stocks;
			      DELETE FROM MutualFunds;
			      DELETE FROM Exchanges; 
			      DELETE FROM MarketIndexes;
                ");
            }
            catch (Exception exp)
            {
                return OperationStatus.CreateFromException(exp.Message, exp);
            }
            return opStatus;            
        }
    }
}

