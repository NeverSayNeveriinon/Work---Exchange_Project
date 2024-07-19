using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyDTO;

// TODO: Change AlowedValues
public class CurrencyRequest
{
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
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