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

    public async Task<(bool isValid, string? message, CurrencyResponse? obj)> AddCurrency(CurrencyRequest? currencyRequest)
    {
        // 'currencyRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyRequest,"The 'CurrencyRequest' object parameter is Null");
        
        var currency = currencyRequest.ToCurrency();
        var currencyResponseByType = await _currencyRepository.GetCurrencyByCurrencyTypeAsync(currency.CurrencyType);
        if (currencyResponseByType is not null) // if currencyResponseByType has sth, means this currencytype already exists
            return (false, "There is Already a Currency Object With This 'Currency Type'", null);
        
        var currencyReturned = await _currencyRepository.AddCurrencyAsync(currency);
        await _currencyRepository.SaveChangesAsync();

        var currencyResponse = currencyReturned.ToCurrencyResponse();
        return (true, null, currencyResponse);
    }   

    public async Task<List<CurrencyResponse>> GetAllCurrencies()
    {
        var currencies = await _currencyRepository.GetAllCurrenciesAsync();
        
        var currencyResponses = currencies.Select(accountItem => accountItem.ToCurrencyResponse()).ToList();
        return currencyResponses;
    }

    public async Task<CurrencyResponse?> GetCurrencyByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Currency'Id' parameter is Null");

        var currency = await _currencyRepository.GetCurrencyByIDAsync(Id.Value);

        // if 'Id' doesn't exist in 'currencies list' 
        if (currency == null) return null;

        var currencyResponse = currency.ToCurrencyResponse();
        return currencyResponse;
    }
    
    public async Task<CurrencyResponse?> GetCurrencyByCurrencyType(string? currencyType)
    {
        // if 'currencyType' is null
        ArgumentNullException.ThrowIfNull(currencyType,"The 'currencyType' parameter is Null");

        var currencyTypeOption = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), currencyType);
        var currency = await _currencyRepository.GetCurrencyByCurrencyTypeAsync(currencyTypeOption);

        // if 'currencyType' doesn't exist in 'currencies list' 
        if (currency == null) return null;

        var currencyResponse = currency.ToCurrencyResponse();
        return currencyResponse;
    }   
    
    public async Task<bool?> DeleteCurrency(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Currency'ID' parameter is Null");

        var currency = await _currencyRepository.GetCurrencyByIDAsync(Id.Value);
        
        // if 'Id' doesn't exist in 'currencies list' 
        if (currency == null) return null;
    
        bool result = _currencyRepository.DeleteCurrency(currency);
        await _currencyRepository.SaveChangesAsync();

        return result;
    }
}