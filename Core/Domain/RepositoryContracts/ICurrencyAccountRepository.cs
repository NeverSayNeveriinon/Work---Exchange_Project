using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyAccountRepository
{
    // public IQueryable<CurrencyAccount> GetCurrencyAccounts();
    public Task<List<CurrencyAccount>> GetAllCurrencyAccountsAsync();
    public Task<List<CurrencyAccount>> GetAllCurrencyAccountsByUserAsync(Guid ownerID);
    public Task<CurrencyAccount?> GetCurrencyAccountByNumberAsync(string number);
    public Task<CurrencyAccount> AddCurrencyAccountAsync(CurrencyAccount account);
    public void DeleteCurrencyAccount(CurrencyAccount account);
    public Task<int> SaveChangesAsync();
    public CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal,decimal,decimal> calculationFunc);
    public CurrencyAccount UpdateStashBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal, decimal, decimal> calculationFunc);
}