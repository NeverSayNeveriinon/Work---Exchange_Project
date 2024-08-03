using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.CurrencyDTO;
using Core.DTO.ExchangeValueDTO;
using Core.Enums;
using Core.Helpers;
using Core.ServiceContracts;
using FluentResults;
using static Core.Helpers.FluentResultsExtensions;

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

    public async Task<Result<ExchangeValueResponse>> AddExchangeValue(ExchangeValueAddRequest exchangeValueAddRequest)
    {
        ArgumentNullException.ThrowIfNull(exchangeValueAddRequest, $"The '{nameof(exchangeValueAddRequest)}' object parameter is Null");
        
        // if 'currencyType's doesn't exist in 'currencies list' 
        var (firstCurrencyResult, firstCurrency) = (await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.FirstCurrencyType)).DeconstructObject();
        if (firstCurrencyResult.IsFailed) return firstCurrencyResult.ToResult(); // if 'first currency' doesn't exist
        var (secondCurrencyResult, secondCurrency) = (await _currencyService.GetCurrencyByCurrencyType(exchangeValueAddRequest.SecondCurrencyType)).DeconstructObject();
        if (secondCurrencyResult.IsFailed) return secondCurrencyResult.ToResult(); // if 'second currency' doesn't exist

        var exchangeValueResponseByCurrenciesID = await _exchangeValueRepository.GetExchangeValueByCurrenciesIDAsync(firstCurrency.Id, secondCurrency.Id);
        if (exchangeValueResponseByCurrenciesID is not null) // if exchangeValueResponseByCurrenciesID has sth, means this two currency id's already exists
            return Result.Fail("There is Already a Exchange Value Object With These First Currency type and Second Currency type");
            
        var exchangeValue = exchangeValueAddRequest.ToExchangeValue(firstCurrency.Id,secondCurrency.Id);
        var oppositeExchangeValue = exchangeValueAddRequest.ToOppositeExchangeValue(firstCurrency.Id,secondCurrency.Id);
            
        var exchangeValueReturned = await _exchangeValueRepository.AddExchangeValueAsync(exchangeValue);
        var oppositeExchangeValueReturned = await _exchangeValueRepository.AddExchangeValueAsync(oppositeExchangeValue);
        var numberOfRowsAffected = await _exchangeValueRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(exchangeValueReturned.ToExchangeValueResponse());
    }   
    

    public async Task<List<ExchangeValueResponse>> GetAllExchangeValues()
    {
        var exchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        
        var exchangeValueResponses = exchangeValues.Select(accountItem => accountItem.ToExchangeValueResponse()).ToList();
        return exchangeValueResponses;
    }

    public async Task<Result<ExchangeValueResponse>> GetExchangeValueByID(int id)
    {
        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(id);
        if (exchangeValue == null) return Result.Fail(CreateNotFoundError("!!An Exchange Value With This ID Has Not Been Found!!")); // if 'id' doesn't exist in 'exchangeValues list' 
        
        return exchangeValue.ToExchangeValueResponse();
    }

    public async Task<Result<decimal>> GetExchangeValueByCurrencyTypes(string firstCurrencyType, string secondCurrencyType)
    {
        ArgumentNullException.ThrowIfNull(firstCurrencyType,$"The '{nameof(firstCurrencyType)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(secondCurrencyType,$"The '{nameof(secondCurrencyType)}' parameter is Null");

        if (firstCurrencyType == secondCurrencyType) return Result.Ok(1M);
        
        // if 'currencyType's doesn't exist in 'currencies list' 
        var (sourceCurrencyResult, sourceCurrencyResponse) = (await _currencyService.GetCurrencyByCurrencyType(firstCurrencyType)).DeconstructObject();
        if (sourceCurrencyResult.IsFailed) return sourceCurrencyResult.ToResult(); // if 'source currency' doesn't exist
        var (destCurrencyResult, destCurrencyResponse) = (await _currencyService.GetCurrencyByCurrencyType(secondCurrencyType)).DeconstructObject();
        if (destCurrencyResult.IsFailed) return destCurrencyResult.ToResult(); // if 'dest currency' doesn't exist
        
        var valueToBeMultiplied = await _exchangeValueRepository.GetUnitValueByCurrencyTypeAsync(sourceCurrencyResponse.Id, destCurrencyResponse.Id);
        if (valueToBeMultiplied == null) return Result.Fail(CreateNotFoundError("!!An Exchange Value With This Currency Types Has Not Been Found!!"));
         
        return Result.Ok(valueToBeMultiplied.Value);
    }
    
    public async Task<Result<ExchangeValueResponse>> UpdateExchangeValueByID(ExchangeValueUpdateRequest exchangeValueUpdateRequest, int exchangeValueID)
    {
        ArgumentNullException.ThrowIfNull(exchangeValueUpdateRequest,$"The '{nameof(exchangeValueUpdateRequest)}' object parameter is Null");
        
        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(exchangeValueID);
        if (exchangeValue == null) return Result.Fail(CreateNotFoundError("!!An Exchange Value With This ID Has Not Been Found!!")); // if 'id' doesn't exist in 'exchangeValues list' 
        
        var allExchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        var oppositeExchangeValue = allExchangeValues.FirstOrDefault(exValue => exValue.SecondCurrencyId == exchangeValue.FirstCurrencyId && exValue.FirstCurrencyId == exchangeValue.SecondCurrencyId);
        if (oppositeExchangeValue == null) return Result.Fail(CreateNotFoundError("!!An Opposite Exchange Value With This ID Has Not Been Found!!")); // if 'id' doesn't exist in 'exchangeValues list' 

        var updatedExchangeValue = _exchangeValueRepository.UpdateExchangeValueByID(exchangeValue, exchangeValueUpdateRequest.ToExchangeValue());
        var updatedOppositeExchangeValue = _exchangeValueRepository.UpdateExchangeValueByID(oppositeExchangeValue, exchangeValueUpdateRequest.ToOppositeExchangeValue());
        var numberOfRowsAffected = await _exchangeValueRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(updatedExchangeValue.ToExchangeValueResponse());
    }

    public async Task<Result> DeleteExchangeValueByID(int id)
    {
        var exchangeValue = await _exchangeValueRepository.GetExchangeValueByIDAsync(id);
        if (exchangeValue == null) return Result.Fail(CreateNotFoundError("!!An Exchange Value With This ID Has Not Been Found!!")); // if 'id' doesn't exist in 'exchangeValues list' 
        
        var allExchangeValues = await _exchangeValueRepository.GetAllExchangeValuesAsync();
        var oppositeExchangeValue = allExchangeValues.FirstOrDefault(exValue => exValue.SecondCurrencyId == exchangeValue.FirstCurrencyId && exValue.FirstCurrencyId == exchangeValue.SecondCurrencyId);
        if (oppositeExchangeValue == null) return Result.Fail(CreateNotFoundError("!!An Opposite Exchange Value With This ID Has Not Been Found!!")); // if 'id' doesn't exist in 'exchangeValues list' 

        _exchangeValueRepository.DeleteExchangeValueByID(exchangeValue);
        _exchangeValueRepository.DeleteExchangeValueByID(oppositeExchangeValue);
        var numberOfRowsAffected = await _exchangeValueRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Been Successful");
    }
}