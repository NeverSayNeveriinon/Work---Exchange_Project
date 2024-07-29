using Core.Domain.Entities;
using Core.Enums;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetAllCurrenciesAsync();
    
    Task<Currency?> GetCurrencyByIDAsync(int id);
    Task<Currency?> GetCurrencyByCurrencyTypeAsync(string currencyType);
    
    Task<Currency> AddCurrencyAsync(Currency currency);
    void DeleteCurrency(Currency currency);
    Task<int> SaveChangesAsync();
}