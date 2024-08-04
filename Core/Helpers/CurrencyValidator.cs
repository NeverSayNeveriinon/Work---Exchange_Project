using Core.ServiceContracts;

namespace Core.Helpers;

public class CurrencyValidator : ICurrencyValidator
{
    private readonly ICurrencyService _currencyService;
    
    public CurrencyValidator(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }
    
    public async Task<bool> ExistsInCurrentCurrencies(string currencyType)
    {
        var allCurrencies = await _currencyService.GetAllCurrencies();
        return allCurrencies.Select(curr => curr.CurrencyType).ToHashSet().Contains(currencyType);
    }
}