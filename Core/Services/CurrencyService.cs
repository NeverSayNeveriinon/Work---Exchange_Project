using System.Net.Mime;
using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyDTO;
using Core.Enums;
using Core.ServiceContracts;
using FluentResults;
using static Core.Helpers.FluentResultsExtensions;

namespace Core.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;
    
    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<CurrencyResponse>> AddCurrency(CurrencyRequest currencyRequest)
    {
        ArgumentNullException.ThrowIfNull(currencyRequest,$"The '{nameof(currencyRequest)}' object parameter is Null");
        
        var currency = currencyRequest.ToCurrency();
        
        var currencyResponseByType = await _currencyRepository.GetCurrencyByCurrencyTypeAsync(currency.CurrencyType);
        if (currencyResponseByType is not null) // if currencyResponseByType has sth, means this currencytype already exists
            return Result.Fail("There is Already a Currency Object With This 'Currency Type'");
        
        var currencyReturned = await _currencyRepository.AddCurrencyAsync(currency);
        var numberOfRowsAffected = await _currencyRepository.SaveChangesAsync();
        
        if (!(numberOfRowsAffected > 0)) 
            return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(currencyReturned.ToCurrencyResponse());
    }   

    public async Task<List<CurrencyResponse>> GetAllCurrencies()
    {
        var currencies = await _currencyRepository.GetAllCurrenciesAsync();
        
        var currencyResponses = currencies.Select(accountItem => accountItem.ToCurrencyResponse()).ToList();
        return currencyResponses;
    }

    public async Task<Result<CurrencyResponse>> GetCurrencyByID(int id)
    {
        var currency = await _currencyRepository.GetCurrencyByIDAsync(id);
        if (currency == null) // if 'id' doesn't exist in 'currencies list'
            return Result.Fail(CreateNotFoundError("!!A Currency With This ID Has Not Been Found!!"));  
        
        return Result.Ok(currency.ToCurrencyResponse());
    }
    
    public async Task<Result<CurrencyResponse>> GetCurrencyByCurrencyType(string currencyType)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' parameter is Null");
        
        var currency = await _currencyRepository.GetCurrencyByCurrencyTypeAsync(currencyType);
        if (currency == null) // if 'currencyType' doesn't exist in 'currencies list'
            return Result.Fail(CreateNotFoundError("!!A Currency With This currencyType Has Not Been Found!!")); 

        return Result.Ok(currency.ToCurrencyResponse());
    }   
    
    public async Task<Result> DeleteCurrencyByID(int id)
    {
        var currency = await _currencyRepository.GetCurrencyByIDAsync(id);
        if (currency == null)  // if 'id' doesn't exist in 'currencies list'
            return Result.Fail(CreateNotFoundError("!!A Currency With This ID Has Not Been Found!!")); 
        
        _currencyRepository.DeleteCurrency(currency);
        
        var numberOfRowsAffected = await _currencyRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) 
            return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Been Successful");
    }
}