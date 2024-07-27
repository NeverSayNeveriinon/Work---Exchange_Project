using Core.Domain.Entities;
using Core.Enums;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyRepository
{
    public Task<List<Currency>> GetAllCurrenciesAsync();
    public Task<Currency?> GetCurrencyByIDAsync(int id);
    public Task<Currency> AddCurrencyAsync(Currency currency);
    public void DeleteCurrency(Currency currency);
    public Task<int> SaveChangesAsync();
    public Task<Currency?> GetCurrencyByCurrencyTypeAsync(string currencyType);
}