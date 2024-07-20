using Core.DTO.ExchangeValueDTO;

namespace Core.ServiceContracts;

public interface IExchangeValueService
{
    public Task<(bool isValid, string? message, ExchangeValueResponse? obj)> AddExchangeValue(ExchangeValueAddRequest? exchangeValueAddRequest);
    public Task<List<ExchangeValueResponse>> GetAllExchangeValues();
    public Task<ExchangeValueResponse?> GetExchangeValueByID(int? ID);
    public Task<(bool isValid, decimal? USDAmount)> GetUSDExchangeValueByCurrencyType(string? currencyType);
    public Task<ExchangeValueResponse?> UpdateExchangeValue(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID);
    public Task<bool?> DeleteExchangeValue(int? ID);
}