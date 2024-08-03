using System.ComponentModel.DataAnnotations;
using Core.Helpers;

namespace Core.DTO.ServicesDTO.Money;

public class MoneyRequest
{
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue, ErrorMessage = "The 'Amount' Must Be Positive")]
    public decimal? Amount { get; init; }
    
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string CurrencyType { get; init; }
}