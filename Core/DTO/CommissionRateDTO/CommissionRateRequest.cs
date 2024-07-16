using Core.Domain.Entities;

namespace Core.DTO.CommissionRateDTO;

public class CommissionRateRequest
{
    public decimal MaxUSDRange { get; set; }
    public decimal CRate { get; set; }
}

public static partial class CommissionRateExtensions
{
    public static CommissionRate ToCommissionRate (this CommissionRateRequest commissionRate)
    {
        CommissionRate response = new CommissionRate()
        {
            Id = 0,
            MaxUSDRange = commissionRate.MaxUSDRange,
            CRate = commissionRate.CRate
        };

        return response;
    }
}