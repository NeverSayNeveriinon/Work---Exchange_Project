using Core.ServiceContracts;

namespace Core.Helpers;

public class Validator : IValidator
{
    private readonly ICurrencyService _currencyService;
    
    public Validator(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }
    
    public async Task<bool> ExistsInCurrentCurrencies(string currencyType)
    {
        var allCurrencies = await _currencyService.GetAllCurrencies();
        return allCurrencies.Select(curr => curr.CurrencyType).ToHashSet().Contains(currencyType);
    }
}