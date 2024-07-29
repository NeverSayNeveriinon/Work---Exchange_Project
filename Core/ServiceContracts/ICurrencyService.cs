using Core.DTO.CurrencyDTO;

namespace Core.ServiceContracts;

public interface ICurrencyService
{
    Task<(bool isValid, string? message, CurrencyResponse? obj)> AddCurrency(CurrencyRequest currencyRequest);
    Task<List<CurrencyResponse>> GetAllCurrencies();
    Task<CurrencyResponse?> GetCurrencyByID(int id);
    Task<CurrencyResponse?> GetCurrencyByCurrencyType(string currencyType);
    Task<(bool, string? message)> DeleteCurrencyByID(int id);
}