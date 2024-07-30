using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Helpers;

namespace Core.DTO.Money;

public class MoneyOpenAccountRequest
{
    [Required(ErrorMessage = "The 'Amount' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'Amount' Must Be Positive")]
    public decimal? Amount { get; set; }
    
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string CurrencyType { get; set; }
}