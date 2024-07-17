using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;

namespace Core.DTO.CommissionRateDTO;

public class CommissionRateRequest
{
    [Required(ErrorMessage = "The 'MaxUSDRange' Can't Be Blank!!!")]
    public decimal? MaxUSDRange { get; set; }
    
    [Required(ErrorMessage = "The 'Commission Rate' Can't Be Blank!!!")]
    [Range(0,100)]
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