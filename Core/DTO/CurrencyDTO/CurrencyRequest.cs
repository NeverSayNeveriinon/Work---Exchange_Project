using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyDTO;

public class CurrencyRequest
{
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string CurrencyType { get; set; } 
}

public static partial class CurrencyExtensions
{
    public static Currency ToCurrency(this CurrencyRequest currencyRequest)
    {
        Currency currency = new Currency()
        {
            Id = 0,
            CurrencyType = currencyRequest.CurrencyType
        };
        return currency;
    }
}