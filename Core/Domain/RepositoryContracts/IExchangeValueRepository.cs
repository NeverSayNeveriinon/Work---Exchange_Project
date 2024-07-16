using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface IExchangeValueRepository
{
    public Task<List<ExchangeValue>> GetAllExchangeValues();
    public Task<ExchangeValue?> GetExchangeValueByID(int id);
    public Task<ExchangeValue> AddExchangeValue(ExchangeValue currency);
    public Task<ExchangeValue> UpdateExchangeValue(ExchangeValue currency, ExchangeValue updatedExchangeValue);
    public Task<bool> DeleteExchangeValue(ExchangeValue currency);
    public Task SaveChangesAsync();
}