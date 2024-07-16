using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyDTO;
using Core.ServiceContracts;

namespace Core.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;
    
    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<CurrencyResponse> AddCurrency(CurrencyRequest? currencyRequest)
    {
        // 'currencyRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyRequest,"The 'CurrencyRequest' object parameter is Null");
        
        // ValidationHelper.ModelValidation(currencyRequest);

        // // 'currencyRequest.Name' is Duplicate //
        // // Way 1
        // if ( (await _currencyRepository.GetFilteredCurrencys(currency => currency.Name == currencyRequest.Name))?.Count > 0)
        // {
        //     throw new ArgumentException("The 'Currency Name' is already exists");
        // }
        
        
        // 'currencyRequest.Name' is valid and there is no problem //
        Currency currency = currencyRequest.ToCurrency();
        Currency currencyReturned = await _currencyRepository.AddCurrency(currency);
        await _currencyRepository.SaveChangesAsync();

        return currencyReturned.ToCurrencyResponse();
    }   
    

    public async Task<List<CurrencyResponse>> GetAllCurrencies()
    {
        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        List<Currency> currencies = await _currencyRepository.GetAllCurrencies();
        
        List<CurrencyResponse> currencyResponses = currencies.Select(accountItem => accountItem.ToCurrencyResponse()).ToList();
        return currencyResponses;
    }

    public async Task<CurrencyResponse?> GetCurrencyByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Currency'Id' parameter is Null");

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        Currency? currency = await _currencyRepository.GetCurrencyByID(Id.Value);

        // if 'id' doesn't exist in 'currencies list' 
        if (currency == null)
        {
            return null;
        }

        // if there is no problem
        CurrencyResponse currencyResponse = currency.ToCurrencyResponse();

        return currencyResponse;;
    }

    public async Task<CurrencyResponse?> UpdateCurrency(CurrencyRequest? currencyRequest, int? currencyID)
    {
        // if 'currency ID' is null
        ArgumentNullException.ThrowIfNull(currencyID,"The Currency'ID' parameter is Null");
        
        // if 'currencyRequest' is null
        ArgumentNullException.ThrowIfNull(currencyRequest,"The 'CurrencyRequest' object parameter is Null");

        
        // Validation
        // ValidationHelper.ModelValidation(currencyRequest);

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        Currency? currency = await _currencyRepository.GetCurrencyByID(currencyID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currency == null)
        {
            return null;
        }
            
        Currency updatedCurrency = await _currencyRepository.UpdateCurrency(currency, currencyRequest.ToCurrency());
        await _currencyRepository.SaveChangesAsync();

        return updatedCurrency.ToCurrencyResponse();
    }

    public async Task<bool?> DeleteCurrency(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Currency'ID' parameter is Null");

        Currency? currency = await _currencyRepository.GetCurrencyByID(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currency == null) 
        {
            return null;
        }
    
        bool result = await _currencyRepository.DeleteCurrency(currency);
        await _currencyRepository.SaveChangesAsync();

        return result;
    }
}