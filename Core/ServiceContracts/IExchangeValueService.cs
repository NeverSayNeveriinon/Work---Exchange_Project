using Core.DTO.ExchangeValueDTO;

namespace Core.ServiceContracts;

public interface IExchangeValueService
{
    Task<(bool isValid, string? message, ExchangeValueResponse? obj)> AddExchangeValue(ExchangeValueAddRequest exchangeValueAddRequest);
    Task<List<ExchangeValueResponse>> GetAllExchangeValues();
    Task<ExchangeValueResponse?> GetExchangeValueByID(int id);
    Task<(bool isValid, decimal? valueToBeMultiplied)> GetExchangeValueByCurrencyTypes(string firstCurrencyType, string secondCurrencyType);
    Task<(bool, string? message, ExchangeValueResponse? obj)> UpdateExchangeValueByID(ExchangeValueUpdateRequest exchangeValueUpdateRequest, int exchangeValueID);
    Task<(bool, string? message)> DeleteExchangeValueByID(int id);
}