using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.DTO;

public class MoneyRequest
{
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    public decimal? Amount { get; set; }
    
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    public string CurrencyType { get; set; }
}