using Core.Domain.Entities;
using Core.Enums;

namespace Core.Domain.RepositoryContracts;

public interface ICurrencyRepository
{
    public Task<List<Currency>> GetAllCurrenciesAsync();
    public Task<Currency?> GetCurrencyByIDAsync(int id);
    public Task<Currency> AddCurrencyAsync(Currency currency);
    public bool DeleteCurrency(Currency currency);
    public Task SaveChangesAsync();
    public Task<Currency?> GetCurrencyByCurrencyTypeAsync(CurrencyTypeOptions currencyType);
}