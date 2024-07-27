using Core.Domain.Entities;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueResponse
{
    public int Id { get; set; }

    public int FirstCurrencyId { get; set; }
    public int SecondCurrencyId { get; set; }
    
    public string? FirstCurrencyType { get; set; }
    public string? SecondCurrencyType { get; set; }
    
    public decimal UnitOfFirstValue { get; set; }
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
