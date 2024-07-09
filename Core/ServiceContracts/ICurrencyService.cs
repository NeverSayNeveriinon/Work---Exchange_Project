using Core.DTO.CurrencyDTO;

namespace Core.ServiceContracts;

public interface ICurrencyService
{
    public Task<CurrencyResponse> AddCurrency(CurrencyRequest? currencyRequest);
    public Task<List<CurrencyResponse>> GetAllCurrencies();
    public Task<CurrencyResponse?> GetCurrencyByID(int? ID);
    public Task<CurrencyResponse?> UpdateCurrency(CurrencyRequest? currencyRequest, int? currencyID);
    public Task<bool?> DeleteCurrency(int? ID);
}