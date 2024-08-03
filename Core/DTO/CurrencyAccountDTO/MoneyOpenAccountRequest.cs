using System.ComponentModel.DataAnnotations;
using Core.Helpers;

namespace Core.DTO.CurrencyAccountDTO;

public class MoneyOpenAccountRequest
{
    [Required(ErrorMessage = "The 'Amount' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue, ErrorMessage = "The 'Amount' Must Be Positive")]
    public decimal? Amount { get; init; }
    
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string CurrencyType { get; init; }
}