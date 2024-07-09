using Core.Domain.Entities;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueUpdateRequest
{
    public decimal UnitOfFirstValue { get; set; }
    public decimal UnitOfSecondValue { get; set; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValue ToExchangeValue(this ExchangeValueUpdateRequest exchangeValueAddRequest)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = exchangeValueAddRequest.UnitOfFirstValue,
            UnitOfSecondValue = exchangeValueAddRequest.UnitOfSecondValue
        };

        return exchangeValue;
    }
}