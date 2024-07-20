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
        // 'commissionRateRequest' is Null //
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' object parameter is Null");

        
        // Validation For Valid MaxSDRange (We shouldn't have same MaxUSDRange) //
        var allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        var allUSDRanges = allCommissionRates.Select(commisionRate => commisionRate.MaxUSDRange).ToHashSet();
        if (allUSDRanges.Contains(commissionRateRequest.MaxUSDRange!.Value)) // means there is already a same range, we don't want that 
        {
            return (false, "There is Already a Commission Rate Object With This 'MaxUSDRange'", null);
        }
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var priviousCommissionRate = allCommissionRates.OrderByDescending(commisionRate => commisionRate.MaxUSDRange)
                                                       .FirstOrDefault(commisionRate => commisionRate.MaxUSDRange < commissionRateRequest.MaxUSDRange);
        if (priviousCommissionRate != null && (priviousCommissionRate.CRate < commissionRateRequest.CRate!.Value)) // means first lesser USDRange has lesser CRate, we don't want that
        {
            return (false, "The 'CRate' Can't be More Than the CRate's of Lesser USDRanges", null);
        }
        
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
        // if 'maxRange' is null
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
        // 'commissionRateRequest' is Null //
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' object parameter is Null");
        
        
        // Validation For Valid CRate (More USDRange has to have less CRate) //
        var allCommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        var priviousCommissionRate = allCommissionRates.OrderByDescending(commisionRate => commisionRate.MaxUSDRange)
                                                       .FirstOrDefault(commisionRate => commisionRate.MaxUSDRange < commissionRateRequest.MaxUSDRange);
        if (priviousCommissionRate != null && (priviousCommissionRate.CRate < commissionRateRequest.CRate!.Value)) // means first lesser USDRange has lesser CRate, we don't want that
        {
            return (false, "The 'CRate' Can't be More Than the CRate's of Lesser USDRanges", null);
        }
        
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
        // if 'maxRange' is null
        ArgumentNullException.ThrowIfNull(maxRange,"The CommissionRate'maxRange' parameter is Null");

        var commissionRate = await _commissionRateRepository.GetCommissionRateByMaxRangeAsync(maxRange.Value);
        
        // if 'maxRange' is invalid (doesn't exist)
        if (commissionRate == null) return null;
    
        bool result = _commissionRateRepository.DeleteCommissionRate(commissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return result;
    }
}