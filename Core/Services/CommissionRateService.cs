using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CommissionRateDTO;
using Core.DTO.CurrencyDTO;
using Core.DTO.ServicesDTO.Money;
using Core.Enums;
using Core.Helpers;
using Core.ServiceContracts;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using static Core.Helpers.FluentResultsExtensions;

namespace Core.Services;

public class CommissionRateService : ICommissionRateService
{
    private readonly ICommissionRateRepository _commissionRateRepository;
    
    public CommissionRateService(ICommissionRateRepository commissionRateRepository)
    {
        _commissionRateRepository = commissionRateRepository;
    }

    public async Task<Result<CommissionRateResponse>> AddCommissionRate(CommissionRateRequest commissionRateRequest)
    {
        ArgumentNullException.ThrowIfNull(commissionRateRequest,$"The '{nameof(commissionRateRequest)}' object parameter is Null");
        
        var allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        // Validation For Valid MaxSDRange (We shouldn't have same MaxUSDRange) //
        var usdRangeValidateResult = await ValidateMaxUSDRangeDuplicate(commissionRateRequest.MaxUSDRange!.Value, allCommissionRates);
        if (usdRangeValidateResult.IsFailed) return Result.Fail(usdRangeValidateResult.FirstErrorMessage());
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var cRateValidateResult = await ValidateCRateRange(commissionRateRequest, allCommissionRates);
        if (cRateValidateResult.IsFailed) return Result.Fail(cRateValidateResult.FirstErrorMessage());
        
        var commissionRate = commissionRateRequest.ToCommissionRate();
        var commissionRateReturned = await _commissionRateRepository.AddCommissionRateAsync(commissionRate);
        var numberOfRowsAffected = await _commissionRateRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(commissionRateReturned.ToCommissionRateResponse());
    }   
    
    public async Task<List<CommissionRateResponse>> GetAllCommissionRates()
    {
        var commissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        var commissionRateResponses = commissionRates.Select(accountItem => accountItem.ToCommissionRateResponse()).ToList();
        return commissionRateResponses;
    }    
    
    //todo:check no need to calculate exchangevalue -> instead call its function
    public async Task<Result<decimal>> GetUSDAmountCRate(Money money)
    {
        ArgumentNullException.ThrowIfNull(money,$"The '{nameof(money)}' object parameter is Null");

        if (money.Currency.CurrencyType != Constants.USDCurrency)
        {
            var exchangeValue = money.Currency.FirstExchangeValues?.FirstOrDefault(exValue => exValue.SecondCurrency.CurrencyType == 
                                                                                              Constants.USDCurrency);
            if (exchangeValue == null) return Result.Fail($"There is No Relevant Exchange Value to Convert to '{Constants.USDCurrency}'");
            var valueToBeMultiplied = exchangeValue.UnitOfFirstValue;
            money.Amount *= valueToBeMultiplied;
        }

        var cRate = await _commissionRateRepository.GetCRateByUSDAmountAsync(money.Amount);
        if (cRate == null) return Result.Fail("There is No Relevant Commission Rate for the Amount");

        return Result.Ok(cRate.Value);
    }

    public async Task<Result<CommissionRateResponse>> GetCommissionRateByMaxRange(decimal maxRange)
    {
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange);
        
        // if 'maxRange' doesn't exist in 'CommissionRates list' 
        if (commissionRate == null) return Result.Fail(CreateNotFoundError("!!A Commission Rate With This maxRange Has Not Been Found!!"));
        
        return Result.Ok(commissionRate.ToCommissionRateResponse());
    }

    public async Task<Result<CommissionRateResponse>> UpdateCRateByMaxRange(CommissionRateRequest commissionRateRequest)
    { 
        ArgumentNullException.ThrowIfNull(commissionRateRequest,$"The '{nameof(commissionRateRequest)}' object parameter is Null");
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var validateResult = await ValidateCRateRange(commissionRateRequest);
        if (validateResult.IsFailed) return Result.Fail(validateResult.FirstErrorMessage());
        
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(commissionRateRequest.MaxUSDRange!.Value);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null) return Result.Fail(CreateNotFoundError("!!A Commission Rate With This maxRange Has Not Been Found!!"));
    
        var commissionRateReturned = _commissionRateRepository.UpdateCRate(commissionRate, commissionRateRequest.CRate!.Value);
        var numberOfRowsAffected = await _commissionRateRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");
        
        return Result.Ok(commissionRateReturned.ToCommissionRateResponse());
    }

    public async Task<Result> DeleteCommissionRateByMaxRange(decimal maxRange)
    {
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null) return Result.Fail(CreateNotFoundError("!!A Commission Rate With This maxRange Has Not Been Found!!"));
    
        _commissionRateRepository.DeleteCommissionRate(commissionRate);
        var numberOfRowsAffected = await _commissionRateRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Been Successful");
    }

    
    
    // Private Methods //
    
    private async Task<Result> ValidateCRateRange(CommissionRateRequest commissionRateRequest, List<CommissionRate>? allCommissionRates = null)
    {
        ArgumentNullException.ThrowIfNull(commissionRateRequest,$"The '{nameof(commissionRateRequest)}' object parameter is Null");

        if (allCommissionRates is null)
            allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();

        var allCommissionRatesOrdered = allCommissionRates.OrderBy(commisionRate => commisionRate.MaxUSDRange).ToList();
        var commissionRateIndex = allCommissionRatesOrdered.Select(commissionRate => commissionRate.MaxUSDRange)
                                                           .ToList()
                                                           .BinarySearch(commissionRateRequest.MaxUSDRange!.Value);
        commissionRateIndex = int.IsNegative(commissionRateIndex) ? ~commissionRateIndex : commissionRateIndex; 
        
        var previousCommissionRate =  allCommissionRatesOrdered.ElementAtOrDefault(commissionRateIndex - 1);
        var nextCommissionRate =  allCommissionRatesOrdered.ElementAtOrDefault(commissionRateIndex);
        
        if ((previousCommissionRate != null && (previousCommissionRate.CRate < commissionRateRequest.CRate!.Value)) || // means last lesser USDRange has lesser CRate, we don't want that
            (nextCommissionRate != null && (nextCommissionRate.CRate > commissionRateRequest.CRate!.Value))) // means first more USDRange has more CRate, we don't want that
        {
            return Result.Fail("The 'CRate' Can't be More Than the CRate's of Lesser USDRanges\n and Lesser Than than CRate's of More USDRanges");
        }

        return Result.Ok();
    }

    private async Task<Result> ValidateMaxUSDRangeDuplicate(decimal maxUSDRange, List<CommissionRate>? allCommissionRates = null)
    {
        if (allCommissionRates is null)
            allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        var allUSDRanges = allCommissionRates.Select(commisionRate => commisionRate.MaxUSDRange).ToHashSet();
        if (allUSDRanges.Contains(maxUSDRange)) // means there is already a same range, we don't want that 
            return Result.Fail("There is Already a Commission Rate Object With This 'MaxUSDRange'");
        
        return Result.Ok();
    }
}