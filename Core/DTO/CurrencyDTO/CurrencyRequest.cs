using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyDTO;

// TODO: Change Aloedalues
public class CurrencyRequest
{
    [AllowedValues("USD","Euro","Rial", ErrorMessage = "The Value Should be one of these 'USD,Euro,Rial' ")]
    public string CurrencyType { get; set; } 
}

public static partial class CurrencyExtensions
{
    public static Currency ToCurrency(this CurrencyRequest currencyRequest)
    {
        Currency currency = new Currency()
        {
            Id = 0,
            CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), currencyRequest.CurrencyType)
        };
        return currency;
    }
}