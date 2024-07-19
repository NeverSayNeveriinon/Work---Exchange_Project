using Core.DTO.ExchangeValueDTO;

namespace Core.ServiceContracts;

public interface IExchangeValueService
{
    public Task<ExchangeValueResponse> AddExchangeValue(ExchangeValueAddRequest? exchangeValueAddRequest);
    public Task<List<ExchangeValueResponse>> GetAllExchangeValues();
    public Task<ExchangeValueResponse?> GetExchangeValueByID(int? ID);
    public Task<decimal?> GetEquivalentUSDByCurrencyType(string? currencyType);
    public Task<ExchangeValueResponse?> UpdateExchangeValue(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID);
    public Task<bool?> DeleteExchangeValue(int? ID);
}