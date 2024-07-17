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

    public async Task<CommissionRateResponse> AddCommissionRate(CommissionRateRequest? commissionRateRequest)
    {
        // 'commissionRateRequest' is Null //
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' object parameter is Null");
        
        
        // 'commissionRateRequest.Name' is valid and there is no problem //
        var commissionRate = commissionRateRequest.ToCommissionRate();
        CommissionRate commissionRateReturned = await _commissionRateRepository.AddCommissionRateAsync(commissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return commissionRateReturned.ToCommissionRateResponse();
    }   
    
    public async Task<List<CommissionRateResponse>> GetAllCommissionRates()
    {
        List<CommissionRate> CommissionRates = await _commissionRateRepository.GetAllCommissionRatesAsync();
        
        List<CommissionRateResponse> CommissionRateResponses = CommissionRates.Select(accountItem => accountItem.ToCommissionRateResponse()).ToList();
        return CommissionRateResponses;
    }    
    
    public async Task<decimal> GetCRate(Money money)
    {
        var valueToBeMultiplied = money.Currency.FirstExchangeValues?.FirstOrDefault(exValue=> exValue.SecondCurrency.CurrencyType == CurrencyTypeOptions.USD)!.UnitOfFirstValue;
        var usdAmount = money.Amount * valueToBeMultiplied;
        var cRate = await _commissionRateRepository.GetCRateByAmountAsync(usdAmount.Value);
        
        return cRate;
    }

    public async Task<CommissionRateResponse?> GetCommissionRateByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CommissionRate'Id' parameter is Null");
        
        CommissionRate? CommissionRate = await _commissionRateRepository.GetCommissionRateByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'CommissionRates list' 
        if (CommissionRate == null)
        {
            return null;
        }

        // if there is no problem
        CommissionRateResponse CommissionRateResponse = CommissionRate.ToCommissionRateResponse();

        return CommissionRateResponse;;
    }

    public async Task<CommissionRateResponse?> UpdateCommissionRate(int? commissionRateID, CommissionRateRequest? commissionRateRequest)
    { 
        // if 'commissionRateID' is null
        ArgumentNullException.ThrowIfNull(commissionRateID,"The CommissionRate'ID' parameter is Null");
        
        // 'commissionRateRequest' is Null //
        ArgumentNullException.ThrowIfNull(commissionRateRequest,"The 'commissionRateRequest' parameter is Null");
        

        CommissionRate? CommissionRate = await _commissionRateRepository.GetCommissionRateByIDAsync(commissionRateID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (CommissionRate == null)
        {
            return null;
        }
         
    
        CommissionRate updatedCommissionRateReturned = _commissionRateRepository.UpdateCommissionRate(CommissionRate, commissionRateRequest.ToCommissionRate());
        await _commissionRateRepository.SaveChangesAsync();

        return updatedCommissionRateReturned.ToCommissionRateResponse();
    }

    public async Task<bool?> DeleteCommissionRate(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CommissionRate'ID' parameter is Null");

        CommissionRate? CommissionRate = await _commissionRateRepository.GetCommissionRateByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (CommissionRate == null) 
        {
            return null;
        }
    
        bool result = _commissionRateRepository.DeleteCommissionRate(CommissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return result;
    }
}