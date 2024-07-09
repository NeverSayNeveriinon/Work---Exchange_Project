using Core.Domain.Entities;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueAddRequest
{
    public int FirstCurrencyId { get; set; }
    public int SecondCurrencyId { get; set; }
    
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
            FirstCurrencyId = exchangeValueAddRequest.FirstCurrencyId,
            SecondCurrencyId = exchangeValueAddRequest.SecondCurrencyId
        };

        return exchangeValue;
    }
}