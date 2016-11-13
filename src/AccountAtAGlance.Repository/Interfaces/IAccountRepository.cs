using AccountAtAGlance.Model;
using System.Threading.Tasks;

namespace AccountAtAGlance.Repository.Interfaces
{
    public interface IAccountRepository
    {
        Task<BrokerageAccount> GetAccountAsync(string acctNumber);
        //BrokerageAccount GetAccount(int id);
        Task<Customer> GetCustomerAsync(string custId);
        Task<OperationStatus> CreateCustomerAsync();

        Task<OperationStatus> CreateAccountPositionsAsync();

        //Task<OperationStatus> InsertAccountAsync(BrokerageAccount acct);
    }
}