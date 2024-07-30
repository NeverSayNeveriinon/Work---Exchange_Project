using Core.DTO;
using Core.DTO.CommissionRateDTO;
using Core.DTO.Money;

namespace Core.ServiceContracts;

public interface ICommissionRateService
{
    Task<(bool isValid, string? message, CommissionRateResponse? obj)> AddCommissionRate(CommissionRateRequest commissionRateRequest);
    Task<List<CommissionRateResponse>> GetAllCommissionRates();
    Task<CommissionRateResponse?> GetCommissionRateByMaxRange(decimal maxRange);
    Task<decimal?> GetUSDAmountCRate(Money money);
    Task<(bool isValid, string? message, CommissionRateResponse? obj)> UpdateCRateByMaxRange(CommissionRateRequest commissionRateRequest);
    Task<(bool, string? message)> DeleteCommissionRateByMaxRange(decimal maxRange);
}