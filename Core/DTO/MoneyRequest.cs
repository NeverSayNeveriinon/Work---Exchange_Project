using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Helpers;

namespace Core.DTO;

public class MoneyRequest
{
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'Amount' Must Be Positive")]
    public decimal? Amount { get; set; }
    
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string CurrencyType { get; set; }
}