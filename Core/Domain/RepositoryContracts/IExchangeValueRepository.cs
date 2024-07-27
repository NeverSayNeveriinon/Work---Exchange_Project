using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface IExchangeValueRepository
{
    public Task<List<ExchangeValue>> GetAllExchangeValuesAsync();
    public Task<ExchangeValue?> GetExchangeValueByIDAsync(int id);
    public Task<ExchangeValue?> GetExchangeValueByCurrenciesIDAsync(int firstCurrencyId, int secondCurrencyId);
    public Task<decimal?> GetExchangeValueByCurrencyTypeAsync(int? sourceCurrencyId, int? destCurrencyId);
    public Task<ExchangeValue> AddExchangeValueAsync(ExchangeValue exchangeValue);
    public ExchangeValue UpdateExchangeValueByID(ExchangeValue exchangeValue, ExchangeValue updatedExchangeValue);
    public void DeleteExchangeValueByID(ExchangeValue exchangeValue);
    public Task<int> SaveChangesAsync();
    public void DetachExchangeValue(ExchangeValue exchangeValue);
}