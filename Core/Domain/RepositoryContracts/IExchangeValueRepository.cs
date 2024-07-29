using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface IExchangeValueRepository
{
    Task<List<ExchangeValue>> GetAllExchangeValuesAsync();
    
    Task<ExchangeValue?> GetExchangeValueByIDAsync(int id);
    Task<ExchangeValue?> GetExchangeValueByCurrenciesIDAsync(int firstCurrencyId, int secondCurrencyId);
    Task<decimal?> GetUnitValueByCurrencyTypeAsync(int? sourceCurrencyId, int? destCurrencyId);
    
    Task<ExchangeValue> AddExchangeValueAsync(ExchangeValue exchangeValue);
    ExchangeValue UpdateExchangeValueByID(ExchangeValue exchangeValue, ExchangeValue updatedExchangeValue);
    void DeleteExchangeValueByID(ExchangeValue exchangeValue);
    Task<int> SaveChangesAsync();
    
    void DetachExchangeValue(ExchangeValue exchangeValue);
}