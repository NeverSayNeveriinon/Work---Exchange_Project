using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CommissionRateDTO;
using Core.DTO.CurrencyDTO;
using Core.Enums;
using Core.Helpers;
using Core.ServiceContracts;

namespace Core.Services;

public class CommissionRateService : ICommissionRateService
{
    private readonly ICommissionRateRepository _commissionRateRepository;
    
    public CommissionRateService(ICommissionRateRepository commissionRateRepository)
    {
        _commissionRateRepository = commissionRateRepository;
    }

    public async Task<(bool isValid, string? message, CommissionRateResponse? obj)> AddCommissionRate(CommissionRateRequest commissionRateRequest)
    {
        ArgumentNullException.ThrowIfNull(commissionRateRequest,$"The '{nameof(commissionRateRequest)}' object parameter is Null");
        
        var allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        // Validation For Valid MaxSDRange (We shouldn't have same MaxUSDRange) //
        var isValidUSDRange = await ValidateMaxUSDRangeDuplicate(commissionRateRequest.MaxUSDRange!.Value, allCommissionRates);
        if (!isValidUSDRange.isValid)
            return (false, isValidUSDRange.message, null);
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var isValidCRate = await ValidateCRateRange(commissionRateRequest, allCommissionRates);
        if (!isValidCRate.isValid)
            return (false, isValidCRate.message, null);
        
        var commissionRate = commissionRateRequest.ToCommissionRate();
        var commissionRateReturned = await _commissionRateRepository.AddCommissionRateAsync(commissionRate);
        var numberOfRowsAffected = await _commissionRateRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again", null);
        
        return (true, null, commissionRateReturned.ToCommissionRateResponse());
    }   
    
    public async Task<List<CommissionRateResponse>> GetAllCommissionRates()
    {
        var commissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        var commissionRateResponses = commissionRates.Select(accountItem => accountItem.ToCommissionRateResponse()).ToList();
        return commissionRateResponses;
    }    
    
    public async Task<decimal?> GetUSDAmountCRate(Money money)
    {
        ArgumentNullException.ThrowIfNull(money,$"The '{nameof(money)}' object parameter is Null");

        if (money.Currency.CurrencyType != Constants.USDCurrency)
        {
            var exchangeValue = money.Currency.FirstExchangeValues?.FirstOrDefault(exValue => exValue.SecondCurrency.CurrencyType == 
                                                                                              Constants.USDCurrency);
            if (exchangeValue == null) return null;
            var valueToBeMultiplied = exchangeValue.UnitOfFirstValue;
            money.Amount *= valueToBeMultiplied;
        }

        var cRate = await _commissionRateRepository.GetCRateByUSDAmountAsync(money.Amount);
        if (cRate == null) return null;

        return cRate;
    }

    public async Task<CommissionRateResponse?> GetCommissionRateByMaxRange(decimal maxRange)
    {
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange);

        // if 'maxRange' doesn't exist in 'CommissionRates list' 
        if (commissionRate == null)  return null;

        return commissionRate.ToCommissionRateResponse();
    }

    public async Task<(bool isValid, string? message, CommissionRateResponse? obj)> UpdateCRateByMaxRange(CommissionRateRequest commissionRateRequest)
    { 
        ArgumentNullException.ThrowIfNull(commissionRateRequest,$"The '{nameof(commissionRateRequest)}' object parameter is Null");
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var (isValid, message) = await ValidateCRateRange(commissionRateRequest);
        if (!isValid)
            return (false, message, null);
        
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(commissionRateRequest.MaxUSDRange!.Value);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null)  return (false, null, null);
    
        var commissionRateReturned = _commissionRateRepository.UpdateCRate(commissionRate, commissionRateRequest.CRate!.Value);
        var numberOfRowsAffected = await _commissionRateRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        return (true, null, commissionRateReturned.ToCommissionRateResponse());
    }

    public async Task<(bool, string? message)> DeleteCommissionRateByMaxRange(decimal maxRange)
    {
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null) return (false, null);
    
        _commissionRateRepository.DeleteCommissionRate(commissionRate);
        var numberOfRowsAffected = await _commissionRateRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again");

        return (true, null);
    }

    
    
    // Private Methods //
    
    private async Task<(bool isValid, string? message)> ValidateCRateRange(CommissionRateRequest commissionRateRequest, List<CommissionRate>? allCommissionRates = null)
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
            return (false, "The 'CRate' Can't be More Than the CRate's of Lesser USDRanges\n and Lesser Than than CRate's of More USDRanges");
        }

        return (true, null);
    }

    private async Task<(bool isValid, string? message)> ValidateMaxUSDRangeDuplicate(decimal maxUSDRange, List<CommissionRate>? allCommissionRates = null)
    {
        if (allCommissionRates is null)
            allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        var allUSDRanges = allCommissionRates.Select(commisionRate => commisionRate.MaxUSDRange).ToHashSet();
        if (allUSDRanges.Contains(maxUSDRange)) // means there is already a same range, we don't want that 
            return (false, "There is Already a Commission Rate Object With This 'MaxUSDRange'");
        
        return (true, null);
    }
}