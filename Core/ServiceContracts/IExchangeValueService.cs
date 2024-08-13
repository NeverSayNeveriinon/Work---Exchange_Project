using Core.DTO.ExchangeValueDTO;
using FluentResults;

namespace Core.ServiceContracts;

public interface IExchangeValueService
{
    Task<Result<ExchangeValueResponse>> AddExchangeValue(ExchangeValueAddRequest exchangeValueAddRequest);
    Task<List<ExchangeValueResponse>> GetAllExchangeValues();
    Task<Result<ExchangeValueResponse>> GetExchangeValueByID(int id);
    Task<Result<decimal>> GetExchangeRateByCurrencyTypes(string firstCurrencyType, string secondCurrencyType);
    Task<Result<ExchangeValueResponse>> UpdateExchangeValueByID(ExchangeValueUpdateRequest exchangeValueUpdateRequest, int exchangeValueID);
    Task<Result> DeleteExchangeValueByID(int id);
}