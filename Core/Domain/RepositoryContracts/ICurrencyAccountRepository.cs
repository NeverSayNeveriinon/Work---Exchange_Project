using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyAccountRepository
{
    public Task<List<CurrencyAccount>> GetAllCurrencyAccountsAsync();
    public Task<CurrencyAccount?> GetCurrencyAccountByNumberAsync(string number);
    public Task<CurrencyAccount> AddCurrencyAccountAsync(CurrencyAccount account);
    public CurrencyAccount UpdateCurrencyAccount(CurrencyAccount account, CurrencyAccount updatedCurrencyAccount);
    public bool DeleteCurrencyAccount(CurrencyAccount account);
    public Task SaveChangesAsync();
    public CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal, decimal, decimal> calculationFunc);
}