using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyRepository
{
    public Task<List<Currency>> GetAllCurrenciesAsync();
    public Task<Currency?> GetCurrencyByIDAsync(int id);
    public Task<Currency> AddCurrencyAsync(Currency currency);
    public Currency UpdateCurrency(Currency currency, Currency updatedCurrency);
    public bool DeleteCurrency(Currency currency);
    public Task SaveChangesAsync();
}