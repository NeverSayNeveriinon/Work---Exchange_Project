﻿using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.ExchangeValueDTO;
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

    public async Task<ExchangeValueResponse> AddExchangeValue(ExchangeValueAddRequest? exchangeValueAddRequest)
    {
        // 'exchangeValueRequest' is Null //
        ArgumentNullException.ThrowIfNull(exchangeValueAddRequest,"The 'ExchangeValueRequest' object parameter is Null");
        
        // 'exchangeValueRequest.Name' is valid and there is no problem //
        var firstCurrency = await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.FirstCurrencyType);
        var secondCurrency = await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.SecondCurrencyType);
        ExchangeValue exchangeValue = exchangeValueAddRequest.ToExchangeValue(firstCurrency.Id,secondCurrency.Id);
        
        ExchangeValue exchangeValueReturned = await _exchangeValueRepository.AddExchangeValueAsync(exchangeValue);
        await _exchangeValueRepository.SaveChangesAsync();

        return exchangeValueReturned.ToExchangeValueResponse();
    }   
    

    public async Task<List<ExchangeValueResponse>> GetAllExchangeValues()
    {
        List<ExchangeValue> exchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        
        List<ExchangeValueResponse> exchangeValueResponses = exchangeValues.Select(accountItem => accountItem.ToExchangeValueResponse()).ToList();
        return exchangeValueResponses;
    }

    public async Task<ExchangeValueResponse?> GetExchangeValueByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'Id' parameter is Null");

        ExchangeValue? exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'exchangeValues list' 
        if (exchangeValue == null)
        {
            return null;
        }

        // if there is no problem
        ExchangeValueResponse exchangeValueResponse = exchangeValue.ToExchangeValueResponse();

        return exchangeValueResponse;;
    }

    public async Task<ExchangeValueResponse?> UpdateExchangeValue(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID)
    {
        // if 'exchangeValue ID' is null
        ArgumentNullException.ThrowIfNull(exchangeValueID,"The ExchangeValue'ID' parameter is Null");
        
        // if 'exchangeValueRequest' is null
        ArgumentNullException.ThrowIfNull(exchangeValueUpdateRequest,"The 'ExchangeValueRequest' object parameter is Null");
        

        ExchangeValue? exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(exchangeValueID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null)
        {
            return null;
        }
            
        ExchangeValue updatedExchangeValue = _exchangeValueRepository.UpdateExchangeValue(exchangeValue, exchangeValueUpdateRequest.ToExchangeValue());
        await _exchangeValueRepository.SaveChangesAsync();

        return updatedExchangeValue.ToExchangeValueResponse();
    }

    public async Task<bool?> DeleteExchangeValue(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'ID' parameter is Null");

        ExchangeValue? exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null) 
        {
            return null;
        }
    
        bool result = _exchangeValueRepository.DeleteExchangeValue(exchangeValue);
        await _exchangeValueRepository.SaveChangesAsync();

        return result;
    }
}