using Core.DTO.CommissionRateDTO;

namespace Core.ServiceContracts;

public interface ICommissionRateService
{
    public Task<CommissionRateResponse> AddCommissionRate(CommissionRateRequest? commissionRateRequest);
    public Task<List<CommissionRateResponse>> GetAllCommissionRates();
    public Task<CommissionRateResponse?> GetCommissionRateByID(int? Id);
    public Task<CommissionRateResponse?> UpdateCommissionRate(int? CommissionRateID, CommissionRateRequest commissionRateRequest);
    public Task<bool?> DeleteCommissionRate(int? Id);
}