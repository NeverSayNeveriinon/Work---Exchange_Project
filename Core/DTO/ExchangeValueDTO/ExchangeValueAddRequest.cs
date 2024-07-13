using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueAddRequest
{
    public string FirstCurrencyType { get; set; }
    public string SecondCurrencyType { get; set; }
    
    public decimal UnitOfFirstValue { get; set; }
    public decimal UnitOfSecondValue { get; set; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValue ToExchangeValue(this ExchangeValueAddRequest exchangeValueAddRequest)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = exchangeValueAddRequest.UnitOfFirstValue,
            UnitOfSecondValue = exchangeValueAddRequest.UnitOfSecondValue,
            FirstCurrency =  new Currency() { CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), exchangeValueAddRequest.FirstCurrencyType) },
            SecondCurrency =  new Currency() { CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), exchangeValueAddRequest.SecondCurrencyType) }
        };

        return exchangeValue;
    }
}