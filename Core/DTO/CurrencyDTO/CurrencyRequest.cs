using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyDTO;

public class CurrencyRequest
{
    public string CuurrencyType { get; set; }
}

public static partial class CurrencyExtensions
{
    public static Currency ToCurrency(this CurrencyRequest currencyRequest)
    {
        Currency currency = new Currency()
        {
            Id = 0,
            CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), currencyRequest.CuurrencyType)
        };

        return currency;
    }
}