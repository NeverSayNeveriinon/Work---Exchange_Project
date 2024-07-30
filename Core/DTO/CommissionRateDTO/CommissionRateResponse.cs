using Core.Domain.Entities;

namespace Core.DTO.CommissionRateDTO;

public class CommissionRateResponse
{
    public int Id { get; init; }
    public decimal MaxUSDRange { get; init; }
    public decimal CRate { get; init; }
}

public static partial class CommissionRateExtensions
{
    public static CommissionRateResponse ToCommissionRateResponse (this CommissionRate commissionRate)
    {
        CommissionRateResponse response = new CommissionRateResponse()
        {
            Id = commissionRate.Id,
            MaxUSDRange = commissionRate.MaxUSDRange,
            CRate = commissionRate.CRate
        };

        return response;
    }
}