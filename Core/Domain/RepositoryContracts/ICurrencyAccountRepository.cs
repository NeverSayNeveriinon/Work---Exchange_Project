using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyAccountRepository
{
    public Task<List<CurrencyAccount>> GetAllCurrencyAccounts();
    public Task<CurrencyAccount?> GetCurrencyAccountByID(int id);
    public Task<CurrencyAccount> AddCurrencyAccount(CurrencyAccount account);
    public Task<CurrencyAccount> UpdateCurrencyAccount(CurrencyAccount account, CurrencyAccount updatedCurrencyAccount);
    public Task<bool> DeleteCurrencyAccount(CurrencyAccount account);
    public Task SaveChangesAsync();
}