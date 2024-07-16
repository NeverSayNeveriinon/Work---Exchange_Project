using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CommissionRateDTO;
using Core.DTO.CurrencyDTO;
using Core.ServiceContracts;

namespace Core.Services;

public class CommissionRateService : ICommissionRateService
{
    private readonly ICommissionRateRepository _commissionRateRepository;
    
    public CommissionRateService(ICommissionRateRepository commissionRateRepository)
    {
        _commissionRateRepository = commissionRateRepository;
    }

    public async Task<CommissionRateResponse> AddCommissionRate(decimal? MaxUSDRange, double? CRate)
    {
        // 'MaxUSDRange' is Null //
        ArgumentNullException.ThrowIfNull(MaxUSDRange,"The 'MaxUSDRange' parameter is Null");
        
        // 'CRate' is Null //
        ArgumentNullException.ThrowIfNull(CRate,"The 'CRate' parameter is Null");
        
        
        // 'commissionRateRequest.Name' is valid and there is no problem //
        var commissionRate = new CommissionRate()
        {
            Id = 0,
            MaxUSDRange = MaxUSDRange.Value,
            CRate = CRate.Value
        };
        
        CommissionRate commissionRateReturned = await _commissionRateRepository.AddCommissionRate(commissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return commissionRateReturned.ToCommissionRateResponse();
    }   
    
    public async Task<List<CommissionRateResponse>> GetAllCommissionRates()
    {
        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        List<CommissionRate> CommissionRates = await _commissionRateRepository.GetAllCommissionRates();
        
        List<CommissionRateResponse> CommissionRateResponses = CommissionRates.Select(accountItem => accountItem.ToCommissionRateResponse()).ToList();
        return CommissionRateResponses;
    }

    public async Task<CommissionRateResponse?> GetCommissionRateByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CommissionRate'Id' parameter is Null");

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CommissionRate? CommissionRate = await _commissionRateRepository.GetCommissionRateByID(Id.Value);

        // if 'id' doesn't exist in 'CommissionRates list' 
        if (CommissionRate == null)
        {
            return null;
        }

        // if there is no problem
        CommissionRateResponse CommissionRateResponse = CommissionRate.ToCommissionRateResponse();

        return CommissionRateResponse;;
    }

    public async Task<CommissionRateResponse?> UpdateCommissionRate(decimal? MaxUSDRange, double? CRate, int? CommissionRateID)
    { 
        // if 'CommissionRate ID' is null
        ArgumentNullException.ThrowIfNull(CommissionRateID,"The CommissionRate'ID' parameter is Null");
        
        // 'MaxUSDRange' is Null //
        ArgumentNullException.ThrowIfNull(MaxUSDRange,"The 'MaxUSDRange' parameter is Null");
        
        // 'CRate' is Null //
        ArgumentNullException.ThrowIfNull(CRate,"The 'CRate' parameter is Null");
        

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CommissionRate? CommissionRate = await _commissionRateRepository.GetCommissionRateByID(CommissionRateID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (CommissionRate == null)
        {
            return null;
        }
         
        var updatedCommissionRate = new CommissionRate()
        {
            Id = 0,
            MaxUSDRange = MaxUSDRange.Value,
            CRate = CRate.Value
        };
        CommissionRate updatedCommissionRateReturned = await _commissionRateRepository.UpdateCommissionRate(CommissionRate, updatedCommissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return updatedCommissionRateReturned.ToCommissionRateResponse();
    }

    public async Task<bool?> DeleteCommissionRate(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CommissionRate'ID' parameter is Null");

        CommissionRate? CommissionRate = await _commissionRateRepository.GetCommissionRateByID(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (CommissionRate == null) 
        {
            return null;
        }
    
        bool result = await _commissionRateRepository.DeleteCommissionRate(CommissionRate);
        await _commissionRateRepository.SaveChangesAsync();

        return result;
    }
}