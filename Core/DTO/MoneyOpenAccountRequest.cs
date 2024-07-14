using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.DTO;

public class MoneyOpenAccountRequest
{
    public decimal Amount { get; set; }
    [AllowedValues("USD","Euro","Rial", ErrorMessage = "The Value Should be one of these 'USD,Euro,Rial' ")]
    public string CurrencyType { get; set; }
}