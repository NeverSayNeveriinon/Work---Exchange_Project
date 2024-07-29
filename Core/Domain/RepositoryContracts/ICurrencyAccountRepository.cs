using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyAccountRepository
{
    // public IQueryable<CurrencyAccount> GetCurrencyAccounts();
    Task<List<CurrencyAccount>> GetAllCurrencyAccountsAsync();
    Task<List<CurrencyAccount>> GetAllCurrencyAccountsByUserAsync(Guid ownerID);
    
    Task<CurrencyAccount?> GetCurrencyAccountByNumberAsync(string number);
    Task<CurrencyAccount> AddCurrencyAccountAsync(CurrencyAccount account);
    
    CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal,decimal,decimal> calculationFunc);
    CurrencyAccount UpdateStashBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal, decimal, decimal> calculationFunc);
    
    void DeleteCurrencyAccount(CurrencyAccount account);
    Task<int> SaveChangesAsync();
}