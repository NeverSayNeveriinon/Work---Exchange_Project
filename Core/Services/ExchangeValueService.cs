using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyDTO;
using Core.DTO.ExchangeValueDTO;
using Core.Enums;
using Core.ServiceContracts;

namespace Core.Services;


public class ExchangeValueService : IExchangeValueService
{
    private readonly IExchangeValueRepository _exchangeValueRepository;
    private readonly ICurrencyService _currencyService;
    
    public ExchangeValueService(IExchangeValueRepository exchangeValueRepository, ICurrencyService currencyService)
    {
        _exchangeValueRepository = exchangeValueRepository;
        _currencyService = currencyService;
    }

    public async Task<(bool isValid, string? message, ExchangeValueResponse? obj)> AddExchangeValue(ExchangeValueAddRequest? exchangeValueAddRequest)
    {
        // 'exchangeValueRequest' is Null //
        ArgumentNullException.ThrowIfNull(exchangeValueAddRequest,"The 'ExchangeValueRequest' object parameter is Null");
        
        var firstCurrency = await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.FirstCurrencyType);
        var secondCurrency = await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.SecondCurrencyType);
        
        var exchangeValueResponseByCurrenciesID = await _exchangeValueRepository.GetExchangeValueByCurrenciesIDAsync(firstCurrency!.Id, secondCurrency!.Id);
        if (exchangeValueResponseByCurrenciesID is not null) // if exchangeValueResponseByCurrenciesID has sth, means this two currency id's already exists
            return (false, "There is Already a Exchange Value Object With These First Currency type and Second Currency type", null);
            
        var exchangeValue = exchangeValueAddRequest.ToExchangeValue(firstCurrency!.Id,secondCurrency!.Id);
        
        var exchangeValueReturned = await _exchangeValueRepository.AddExchangeValueAsync(exchangeValue);
        await _exchangeValueRepository.SaveChangesAsync();

        var exchangeValueResponse = exchangeValueReturned.ToExchangeValueResponse();
        return (true, null, exchangeValueResponse);
    }   
    

    public async Task<List<ExchangeValueResponse>> GetAllExchangeValues()
    {
        var exchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        
        var exchangeValueResponses = exchangeValues.Select(accountItem => accountItem.ToExchangeValueResponse()).ToList();
        return exchangeValueResponses;
    }

    public async Task<ExchangeValueResponse?> GetExchangeValueByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'Id' parameter is Null");

        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'exchangeValues list' 
        if (exchangeValue == null) return null;

        // if there is no problem
        var exchangeValueResponse = exchangeValue.ToExchangeValueResponse();

        return exchangeValueResponse;
    }

    public async Task<(bool isValid, decimal? USDAmount)> GetUSDExchangeValueByCurrencyType(string? currencyType)
    {
        // if 'currencyType' is null
        ArgumentNullException.ThrowIfNull(currencyType,"The 'currencyType' parameter is Null");

        var sourceCurrencyResponse = await _currencyService.GetCurrencyByCurrencyType(currencyType);
        var usdCurrencyResponse = await _currencyService.GetCurrencyByCurrencyType("USD");

        // if 'currencyType' doesn't exist in 'currencies list' 
        if (sourceCurrencyResponse == null)
            return (false, null);

        var valueToBeMultiplied = await _exchangeValueRepository.GetUSDExchangeValueByCurrencyTypeAsync(sourceCurrencyResponse.Id, usdCurrencyResponse!.Id);
        if (valueToBeMultiplied == null)
            return (false, null);
        
        return (true, valueToBeMultiplied);
    }
    
    public async Task<ExchangeValueResponse?> UpdateExchangeValue(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID)
    {
        // if 'exchangeValue ID' is null
        ArgumentNullException.ThrowIfNull(exchangeValueID,"The ExchangeValue'ID' parameter is Null");
        
        // if 'exchangeValueRequest' is null
        ArgumentNullException.ThrowIfNull(exchangeValueUpdateRequest,"The 'ExchangeValueRequest' object parameter is Null");
        
        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(exchangeValueID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null) return null;
            
        var updatedExchangeValue = _exchangeValueRepository.UpdateExchangeValue(exchangeValue, exchangeValueUpdateRequest.ToExchangeValue());
        await _exchangeValueRepository.SaveChangesAsync();

        return updatedExchangeValue.ToExchangeValueResponse();
    }

    public async Task<bool?> DeleteExchangeValue(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'ID' parameter is Null");

        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null) return null;
    
        bool result = _exchangeValueRepository.DeleteExchangeValue(exchangeValue);
        await _exchangeValueRepository.SaveChangesAsync();

        return result;
    }
}