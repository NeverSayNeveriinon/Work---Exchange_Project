using Core.DTO;
using Core.DTO.CommissionRateDTO;

namespace Core.ServiceContracts;

public interface ICommissionRateService
{
    public Task<(bool isValid, string? message, CommissionRateResponse? obj)> AddCommissionRate(CommissionRateRequest? commissionRateRequest);
    public Task<List<CommissionRateResponse>> GetAllCommissionRates();
    public Task<CommissionRateResponse?> GetCommissionRateByMaxRange(decimal? maxRange);
    public Task<decimal?> GetUSDAmountCRate(Money money);
    public Task<(bool isValid, string? message, CommissionRateResponse? obj)> UpdateCRateByMaxRange(CommissionRateRequest? commissionRateRequest);
    public Task<bool?> DeleteCommissionRateByMaxRange(decimal? maxRange);
}