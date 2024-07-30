using Core.Domain.Entities;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueResponse
{
    public int Id { get; init; }

    public int FirstCurrencyId { get; init; }
    public int SecondCurrencyId { get; init; }
    
    public string? FirstCurrencyType { get; init; }
    public string? SecondCurrencyType { get; init; }
    
    public decimal UnitOfFirstValue { get; init; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValueResponse ToExchangeValueResponse (this ExchangeValue exchangeValue)
    {
        ExchangeValueResponse response = new ExchangeValueResponse()
        {
            Id = exchangeValue.Id,
            UnitOfFirstValue = exchangeValue.UnitOfFirstValue,
            FirstCurrencyId = exchangeValue.FirstCurrencyId,
            SecondCurrencyId = exchangeValue.SecondCurrencyId,
            FirstCurrencyType = exchangeValue.FirstCurrency?.CurrencyType,
            SecondCurrencyType = exchangeValue.SecondCurrency?.CurrencyType,
        };

        return response;
    }
}
