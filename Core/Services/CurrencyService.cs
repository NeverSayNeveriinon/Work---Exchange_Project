using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyDTO;
using Core.Enums;
using Core.ServiceContracts;

namespace Core.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;
    
    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<(bool isValid, string? message, CurrencyResponse? obj)> AddCurrency(CurrencyRequest currencyRequest)
    {
        ArgumentNullException.ThrowIfNull(currencyRequest,$"The '{nameof(currencyRequest)}' object parameter is Null");
        
        var currency = currencyRequest.ToCurrency();
        var currencyResponseByType = await _currencyRepository.GetCurrencyByCurrencyTypeAsync(currency.CurrencyType);
        if (currencyResponseByType is not null) // if currencyResponseByType has sth, means this currencytype already exists
            return (false, "There is Already a Currency Object With This 'Currency Type'", null);
        
        var currencyReturned = await _currencyRepository.AddCurrencyAsync(currency);
        var numberOfRowsAffected = await _currencyRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        return (true, null, currencyReturned.ToCurrencyResponse());
    }   

    public async Task<List<CurrencyResponse>> GetAllCurrencies()
    {
        var currencies = await _currencyRepository.GetAllCurrenciesAsync();
        
        var currencyResponses = currencies.Select(accountItem => accountItem.ToCurrencyResponse()).ToList();
        return currencyResponses;
    }

    public async Task<CurrencyResponse?> GetCurrencyByID(int id)
    {
        var currency = await _currencyRepository.GetCurrencyByIDAsync(id);
        if (currency == null) return null; // if 'id' doesn't exist in 'currencies list' 
        
        return currency.ToCurrencyResponse();
    }
    
    public async Task<CurrencyResponse?> GetCurrencyByCurrencyType(string currencyType)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' parameter is Null");
        
        var currency = await _currencyRepository.GetCurrencyByCurrencyTypeAsync(currencyType);
        if (currency == null) return null; // if 'currencyType' doesn't exist in 'currencies list' 

        return currency.ToCurrencyResponse();
    }   
    
    public async Task<(bool, string? message)> DeleteCurrencyByID(int id)
    {
        var currency = await _currencyRepository.GetCurrencyByIDAsync(id);
        if (currency == null) return (false, null); // if 'id' doesn't exist in 'currencies list' 
    
        _currencyRepository.DeleteCurrency(currency);
        var numberOfRowsAffected = await _currencyRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again");

        return (true, null);
    }
}