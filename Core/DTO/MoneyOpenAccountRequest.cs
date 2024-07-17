using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.DTO;

public class MoneyOpenAccountRequest
{
    [Required(ErrorMessage = "The 'Amount' Can't Be Blank!!!")]
    public decimal? Amount { get; set; }
    
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
    public string CurrencyType { get; set; }
}