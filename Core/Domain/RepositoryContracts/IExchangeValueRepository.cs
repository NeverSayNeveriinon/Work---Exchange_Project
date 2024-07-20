using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface IExchangeValueRepository
{
    public Task<List<ExchangeValue>> GetAllExchangeValuesAsync();
    public Task<ExchangeValue?> GetExchangeValueByIDAsync(int id);
    public Task<ExchangeValue?> GetExchangeValueByCurrenciesIDAsync(int firstCurrencyId, int secondCurrencyId);
    public Task<decimal?> GetUSDExchangeValueByCurrencyTypeAsync(int? sourceCurrencyId, int? usdCurrencyId);
    public Task<ExchangeValue> AddExchangeValueAsync(ExchangeValue exchangeValue);
    public ExchangeValue UpdateExchangeValue(ExchangeValue exchangeValue, ExchangeValue updatedExchangeValue);
    public bool DeleteExchangeValue(ExchangeValue exchangeValue);
    public Task SaveChangesAsync();
}