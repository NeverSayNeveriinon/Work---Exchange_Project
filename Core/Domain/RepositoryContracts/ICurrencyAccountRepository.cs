using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyAccountRepository
{
    public IQueryable<CurrencyAccount> GetCurrencyAccounts();
    // public Task<List<CurrencyAccount>> GetAllCurrencyAccountsByUserAsync(Guid ownerID);
    // public CurrencyAccount? GetCurrencyAccountByNumber(string number);
    public Task<CurrencyAccount> AddCurrencyAccountAsync(CurrencyAccount account);
    public bool DeleteCurrencyAccount(CurrencyAccount account);
    public Task SaveChangesAsync();
    public CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal, decimal, decimal> calculationFunc);
}