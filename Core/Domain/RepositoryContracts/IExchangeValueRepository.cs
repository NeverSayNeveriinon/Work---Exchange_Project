using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface IExchangeValueRepository
{
    public Task<List<ExchangeValue>> GetAllExchangeValuesAsync();
    public Task<ExchangeValue?> GetExchangeValueByIDAsync(int id);
    public Task<decimal?> GetEquivalentUSDByCurrencyTypeAsync(int? sourceCurrencyId, int? usdCurrencyId);
    public Task<ExchangeValue> AddExchangeValueAsync(ExchangeValue currency);
    public ExchangeValue UpdateExchangeValue(ExchangeValue currency, ExchangeValue updatedExchangeValue);
    public bool DeleteExchangeValue(ExchangeValue currency);
    public Task SaveChangesAsync();
}