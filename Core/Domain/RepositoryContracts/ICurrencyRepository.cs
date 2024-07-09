using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyRepository
{
    public Task<List<Currency>> GetAllCurrencies();
    public Task<Currency?> GetCurrencyByID(int id);
    public Task<Currency> AddCurrency(Currency currency);
    public Task<Currency> UpdateCurrency(Currency currency, Currency updatedCurrency);
    public Task<bool> DeleteCurrency(Currency currency);
    public Task SaveChangesAsync();
}