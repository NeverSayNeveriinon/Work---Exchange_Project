using Core.DTO.CurrencyDTO;

namespace Core.ServiceContracts;

public interface ICurrencyService
{
    public Task<(bool isValid, string? message, CurrencyResponse? obj)> AddCurrency(CurrencyRequest? currencyRequest);
    public Task<List<CurrencyResponse>> GetAllCurrencies();
    public Task<CurrencyResponse?> GetCurrencyByID(int? ID);
    public Task<CurrencyResponse?> GetCurrencyByCurrencyType(string? currencyType);
    public Task<(bool, string? message)> DeleteCurrencyByID(int? ID);
}