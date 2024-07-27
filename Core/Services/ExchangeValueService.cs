using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyAccountDTO;
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
        ArgumentNullException.ThrowIfNull(exchangeValueAddRequest,"The 'ExchangeValueRequest' object parameter is Null");
        
        var firstCurrency = await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.FirstCurrencyType);
        var secondCurrency = await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.SecondCurrencyType);
        
        var exchangeValueResponseByCurrenciesID = await _exchangeValueRepository.GetExchangeValueByCurrenciesIDAsync(firstCurrency!.Id, secondCurrency!.Id);
        if (exchangeValueResponseByCurrenciesID is not null) // if exchangeValueResponseByCurrenciesID has sth, means this two currency id's already exists
            return (false, "There is Already a Exchange Value Object With These First Currency type and Second Currency type", null);
            
        var exchangeValue = exchangeValueAddRequest.ToExchangeValue(firstCurrency!.Id,secondCurrency!.Id);
        var oppositeExchangeValue = exchangeValueAddRequest.ToOppositeExchangeValue(firstCurrency!.Id,secondCurrency!.Id);
            
        var exchangeValueReturned = await _exchangeValueRepository.AddExchangeValueAsync(exchangeValue);
        var oppositeExchangeValueReturned = await _exchangeValueRepository.AddExchangeValueAsync(oppositeExchangeValue);
        var numberOfRowsAffected = await _exchangeValueRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

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
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'Id' parameter is Null");

        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'exchangeValues list' 
        if (exchangeValue == null) return null;

        // if there is no problem
        var exchangeValueResponse = exchangeValue.ToExchangeValueResponse();

        return exchangeValueResponse;
    }

    public async Task<(bool isValid, decimal? valueToBeMultiplied)> GetExchangeValueByCurrencyType(string? firstCurrencyType, string? secondCurrencyType)
    {
        ArgumentNullException.ThrowIfNull(firstCurrencyType,"The 'firstCurrencyType' parameter is Null");
        ArgumentNullException.ThrowIfNull(secondCurrencyType,"The 'secondCurrencyType' parameter is Null");

        if (firstCurrencyType == secondCurrencyType)
            return (true, 1);
        
        var sourceCurrencyResponse = await _currencyService.GetCurrencyByCurrencyType(firstCurrencyType);
        var destCurrencyResponse = await _currencyService.GetCurrencyByCurrencyType(secondCurrencyType);

        // if 'currencyType's doesn't exist in 'currencies list' 
        if (sourceCurrencyResponse == null)
            return (false, null);
        if (destCurrencyResponse == null)
            return (false, null);

        var valueToBeMultiplied = await _exchangeValueRepository.GetExchangeValueByCurrencyTypeAsync(sourceCurrencyResponse.Id, destCurrencyResponse!.Id);
        if (valueToBeMultiplied == null)
            return (false, null);
        
        return (true, valueToBeMultiplied);
    }
    
    public async Task<(bool, string? message, ExchangeValueResponse? obj)> UpdateExchangeValueByID(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID)
    {
        ArgumentNullException.ThrowIfNull(exchangeValueID,"The ExchangeValue'ID' parameter is Null");
        ArgumentNullException.ThrowIfNull(exchangeValueUpdateRequest,"The 'ExchangeValueRequest' object parameter is Null");
        
        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(exchangeValueID.Value);
        
        // if 'id' doesn't exist in 'exchangeValues list' 
        if (exchangeValue == null) return (false, null, null);
        
        var allExchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        var oppositeExchangeValue = allExchangeValues.FirstOrDefault(exValue => exValue.SecondCurrencyId == exchangeValue.FirstCurrencyId && exValue.FirstCurrencyId == exchangeValue.SecondCurrencyId);
            
        var updatedExchangeValue = _exchangeValueRepository.UpdateExchangeValueByID(exchangeValue, exchangeValueUpdateRequest.ToExchangeValue());
        var updatedOppositeExchangeValue = _exchangeValueRepository.UpdateExchangeValueByID(oppositeExchangeValue, exchangeValueUpdateRequest.ToOppositeExchangeValue());
        var numberOfRowsAffected = await _exchangeValueRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        return (true, null, updatedExchangeValue.ToExchangeValueResponse());
    }

    public async Task<(bool, string? message)> DeleteExchangeValueByID(int? Id)
    {
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'ID' parameter is Null");

        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null) return (false, null);
        
        var allExchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        var oppositeExchangeValue = allExchangeValues.FirstOrDefault(exValue => exValue.SecondCurrencyId == exchangeValue.FirstCurrencyId && exValue.FirstCurrencyId == exchangeValue.SecondCurrencyId);
        
        _exchangeValueRepository.DeleteExchangeValueByID(exchangeValue);
        _exchangeValueRepository.DeleteExchangeValueByID(oppositeExchangeValue!);
        var numberOfRowsAffected = await _exchangeValueRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return (false, "The Request Has Not Been Done Completely, Try Again");

        return (true, null);
    }
}