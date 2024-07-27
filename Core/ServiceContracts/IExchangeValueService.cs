using Core.DTO.ExchangeValueDTO;

namespace Core.ServiceContracts;

public interface IExchangeValueService
{
    public Task<(bool isValid, string? message, ExchangeValueResponse? obj)> AddExchangeValue(ExchangeValueAddRequest? exchangeValueAddRequest);
    public Task<List<ExchangeValueResponse>> GetAllExchangeValues();
    public Task<ExchangeValueResponse?> GetExchangeValueByID(int? ID);
    public Task<(bool isValid, decimal? valueToBeMultiplied)> GetExchangeValueByCurrencyType(string? firstCurrencyType, string? secondCurrencyType);
    public Task<(bool, string? message, ExchangeValueResponse? obj)> UpdateExchangeValueByID(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID);
    public Task<(bool, string? message)> DeleteExchangeValueByID(int? ID);
}