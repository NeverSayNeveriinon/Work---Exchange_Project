using Core.DTO.CommissionRateDTO;

namespace Core.ServiceContracts;

public interface ICommissionRateService
{
    public Task<CommissionRateResponse> AddCommissionRate(decimal? MaxUSDRange, double? CRate);
    public Task<List<CommissionRateResponse>> GetAllCommissionRates();
    public Task<CommissionRateResponse?> GetCommissionRateByID(int? Id);
    public Task<CommissionRateResponse?> UpdateCommissionRate(decimal? MaxUSDRange, double? CRate, int? CommissionRateID);
    public Task<bool?> DeleteCommissionRate(int? Id);
}