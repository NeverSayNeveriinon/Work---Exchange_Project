using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Helpers;

namespace Core.DTO.CommissionRateDTO;

public class CommissionRateRequest
{
    [Required(ErrorMessage = "The 'MaxUSDRange' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue, ErrorMessage = "The 'MaxUSDRange' Must Be Positive")]
    public decimal? MaxUSDRange { get; set; }

    [Required(ErrorMessage = "The 'Commission Rate' Can't Be Blank!!!")]
    [DecimalRange("0","0.5", ErrorMessage = "The 'CRate' Must Be Between 0 and 0.5")]
    public decimal? CRate { get; set; }
}

public static partial class CommissionRateExtensions
{
    public static CommissionRate ToCommissionRate (this CommissionRateRequest commissionRate)
    {
        CommissionRate response = new CommissionRate()
        {
            Id = 0,
            MaxUSDRange = commissionRate.MaxUSDRange!.Value,
            CRate = commissionRate.CRate!.Value
        };

        return response;
    }
}