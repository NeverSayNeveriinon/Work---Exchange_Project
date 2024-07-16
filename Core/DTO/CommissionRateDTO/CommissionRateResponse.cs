using Core.Domain.Entities;

namespace Core.DTO.CommissionRateDTO;

public class CommissionRateResponse
{
    public int Id { get; set; }
    public decimal MaxUSDRange { get; set; }
    public decimal CRate { get; set; }
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