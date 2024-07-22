using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CommissionRateDTO;
using Core.DTO.CurrencyDTO;
using Core.Enums;
using Core.ServiceContracts;

namespace Core.Services;

public class CommissionRateService : ICommissionRateService
{
    private readonly ICommissionRateRepository _commissionRateRepository;
    
    public CommissionRateService(ICommissionRateRepository commissionRateRepository)
    {
        _commissionRateRepository = commissionRateRepository;
    }

    public async Task<(bool isValid, string? message, CommissionRateResponse? obj)> AddCommissionRate(CommissionRateRequest? commissionRateRequest)
    {
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' object parameter is Null");
        
        var allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        // Validation For Valid MaxSDRange (We shouldn't have same MaxUSDRange) //
        var isValidUSDRange = await ValidateMaxUSDRangeDuplicate(commissionRateRequest.MaxUSDRange, allCommissionRates);
        if (!isValidUSDRange.isValid)
            return (false, isValidUSDRange.message, null);
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var isValidCRate = await ValidateCRateRange(commissionRateRequest, allCommissionRates);
        if (!isValidCRate.isValid)
            return (false, isValidCRate.message, null);
        
        var commissionRate = commissionRateRequest.ToCommissionRate();
        var commissionRateReturned = await _commissionRateRepository.AddCommissionRateAsync(commissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        var commissionRateResponse = commissionRateReturned.ToCommissionRateResponse();
        return (true, null, commissionRateResponse);
    }   
    
    public async Task<List<CommissionRateResponse>> GetAllCommissionRates()
    {
        var CommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        var CommissionRateResponses = CommissionRates.Select(accountItem => accountItem.ToCommissionRateResponse()).ToList();
        return CommissionRateResponses;
    }    
    
    public async Task<decimal?> GetUSDAmountCRate(Money money)
    {
        var valueToBeMultiplied = money.Currency.FirstExchangeValues?.FirstOrDefault(exValue=> exValue.SecondCurrency.CurrencyType == CurrencyTypeOptions.USD)!.UnitOfFirstValue;
        var usdAmount = money.Amount * valueToBeMultiplied;
        var cRate = await _commissionRateRepository.GetCRateByUSDAmountAsync(usdAmount!.Value);
        
        return cRate;
    }

    public async Task<CommissionRateResponse?> GetCommissionRateByMaxRange(decimal? maxRange)
    {
        ArgumentNullException.ThrowIfNull(maxRange,"The CommissionRate'maxRange' parameter is Null");
        
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange.Value);

        // if 'id' doesn't exist in 'CommissionRates list' 
        if (commissionRate == null)  return null;

        // if there is no problem
        var commissionRateResponse = commissionRate.ToCommissionRateResponse();
        return commissionRateResponse;
    }

    public async Task<(bool isValid, string? message, CommissionRateResponse? obj)> UpdateCRateByMaxRange(CommissionRateRequest? commissionRateRequest)
    { 
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' object parameter is Null");
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var (isValid, message) = await ValidateCRateRange(commissionRateRequest);
        if (!isValid)
            return (false, message, null);
        
        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(commissionRateRequest.MaxUSDRange!.Value);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null)  return (false, null, null);
    
        var updatedCommissionRateReturned = _commissionRateRepository.UpdateCRate(commissionRate, commissionRateRequest.CRate!.Value);
        await _commissionRateRepository.SaveChangesAsync();

        var commissionRateResponse = updatedCommissionRateReturned.ToCommissionRateResponse();
        return (true, null, commissionRateResponse);
    }

    public async Task<bool?> DeleteCommissionRateByMaxRange(decimal? maxRange)
    {
        ArgumentNullException.ThrowIfNull(maxRange,"The CommissionRate'maxRange' parameter is Null");

        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange.Value);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null) return null;
    
        bool result = _commissionRateRepository.DeleteCommissionRate(commissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return result;
    }

    
    // Private Methods //
    
    private async Task<(bool isValid, string? message)> ValidateCRateRange(CommissionRateRequest? commissionRateRequest, List<CommissionRate>? allCommissionRates = null)
    {
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' parameter is Null");

        if (allCommissionRates is null)
            allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();

        var allCommissionRatesOrdered = allCommissionRates.OrderBy(commisionRate => commisionRate.MaxUSDRange).ToList();
        var commissionRateIndex = ~(allCommissionRatesOrdered.Select(commissionRate => commissionRate.MaxUSDRange).ToList().BinarySearch(commissionRateRequest.MaxUSDRange!.Value));
        var priviousCommissionRate =  allCommissionRatesOrdered.ElementAtOrDefault(commissionRateIndex - 1);
        var nextCommissionRate =  allCommissionRatesOrdered.ElementAtOrDefault(commissionRateIndex);
        
        if ((priviousCommissionRate != null && (priviousCommissionRate.CRate < commissionRateRequest.CRate!.Value)) || // means last lesser USDRange has lesser CRate, we don't want that
            (nextCommissionRate != null && (nextCommissionRate.CRate > commissionRateRequest.CRate!.Value))) // means first more USDRange has more CRate, we don't want that
        {
            return (false, "The 'CRate' Can't be More Than the CRate's of Lesser USDRanges\n and Lesser Than than CRate's of More USDRanges");
        }

        return (true, null);
    }

    private async Task<(bool isValid, string? message)> ValidateMaxUSDRangeDuplicate(decimal? maxUSDRange, List<CommissionRate>? allCommissionRates = null)
    {
        ArgumentNullException.ThrowIfNull(maxUSDRange,"The 'maxUSDRange' parameter is Null");
        
        if (allCommissionRates is null)
            allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        var allUSDRanges = allCommissionRates.Select(commisionRate => commisionRate.MaxUSDRange).ToHashSet();
        if (allUSDRanges.Contains(maxUSDRange.Value)) // means there is already a same range, we don't want that 
            return (false, "There is Already a Commission Rate Object With This 'MaxUSDRange'");
        
        return (true, null);
    }
}