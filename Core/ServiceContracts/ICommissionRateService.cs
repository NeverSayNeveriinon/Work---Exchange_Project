using Core.DTO;
using Core.DTO.CommissionRateDTO;
using Core.DTO.ServicesDTO;
using FluentResults;

namespace Core.ServiceContracts;

public interface ICommissionRateService
{
    Task<Result<CommissionRateResponse>> AddCommissionRate(CommissionRateRequest commissionRateRequest);
    Task<List<CommissionRateResponse>> GetAllCommissionRates();
    Task<Result<CommissionRateResponse>> GetCommissionRateByMaxRange(decimal maxRange);
    Task<Result<decimal>> GetCRateByMoney(Money money);
    Task<Result<CommissionRateResponse>> UpdateCRateByMaxRange(CommissionRateRequest commissionRateRequest);
    Task<Result> DeleteCommissionRateByMaxRange(decimal maxRange);
}