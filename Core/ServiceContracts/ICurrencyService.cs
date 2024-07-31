using Core.DTO.CurrencyDTO;
using FluentResults;

namespace Core.ServiceContracts;

public interface ICurrencyService
{
    Task<Result<CurrencyResponse>> AddCurrency(CurrencyRequest currencyRequest);
    Task<List<CurrencyResponse>> GetAllCurrencies();
    Task<Result<CurrencyResponse>> GetCurrencyByID(int id);
    Task<Result<CurrencyResponse>> GetCurrencyByCurrencyType(string currencyType);
    Task<Result> DeleteCurrencyByID(int id);
}