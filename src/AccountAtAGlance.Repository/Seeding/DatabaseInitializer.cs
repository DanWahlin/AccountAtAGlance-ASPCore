using AccountAtAGlance.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AccountAtAGlance.Repository.Seeding
{
    public class DatabaseInitializer
    {
        IAccountRepository _AccountRepository;
        ISecurityRepository _SecurityRepository;
        IMarketsAndNewsRepository _MarketsAndNewsRepository;
        AccountAtAGlanceContext _Context;

        public DatabaseInitializer(AccountAtAGlanceContext context, IAccountRepository acctRepo,
            ISecurityRepository securityRepo, IMarketsAndNewsRepository marketsRepo)
        {
            _Context = context;
            _AccountRepository = acctRepo;
            _SecurityRepository = securityRepo;
            _MarketsAndNewsRepository = marketsRepo;
        }

        public async Task SeedAsync()
        {
            var db = _Context.Database;
            if (db != null)
            {
                if (await db.EnsureCreatedAsync())
                {
                    await InsertSampleData();
                }
            }
            else
            {
                await InsertSampleData();
            }
        }

        public async Task InsertSampleData()
        {
            await Task.Run(async () =>
            {
                var deleteSecuritiesExchanges = @"
                    CREATE PROCEDURE dbo.DeleteSecuritiesAndExchanges

                    AS
	                    BEGIN
	 
	 		                    BEGIN TRANSACTION
		                    BEGIN TRY
			                    DELETE FROM Positions;   
			                    DELETE FROM Stocks;
			                    DELETE FROM MutualFunds;
			                    DELETE FROM Exchanges; 
			                    DELETE FROM MarketIndexes;
			                    COMMIT TRANSACTION
			                    SELECT 0				
		                    END TRY
		                    BEGIN CATCH
			                    ROLLBACK TRANSACTION
			                    SELECT -1		
		                    END CATCH
	
	                    END
                    ";

                var deleteAccounts = @"
                    CREATE PROCEDURE dbo.DeleteAccounts

                    AS
	                    BEGIN

		                    BEGIN TRANSACTION
			                    BEGIN TRY
				                    DELETE FROM Orders;                                              
				                    DELETE FROM BrokerageAccounts;
				                    DELETE FROM Customers;					
				                    COMMIT TRANSACTION
				                    SELECT 0				
			                    END TRY
			                    BEGIN CATCH
				                    ROLLBACK TRANSACTION
				                    SELECT -1		
			                    END CATCH
	                    END	
	                ";

                using (var connection = _Context.Database.GetDbConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = deleteSecuritiesExchanges;
                        command.ExecuteNonQuery();

                        command.CommandText = deleteAccounts;
                        command.ExecuteNonQuery();
                    }

                    await _SecurityRepository.InsertSecurityDataAsync();
                    await _MarketsAndNewsRepository.InsertMarketDataAsync();
                    await _AccountRepository.CreateCustomerAsync();
                    await _AccountRepository.CreateAccountPositionsAsync();
                }
            });
        }
    }
}
