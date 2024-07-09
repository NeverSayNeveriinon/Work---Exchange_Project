using Core.Domain.Entities;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueResponse
{
    public int Id { get; set; }

    public int FirstCurrencyId { get; set; }
    public int SecondCurrencyId { get; set; }
    
    public decimal UnitOfFirstValue { get; set; }
    public decimal UnitOfSecondValue { get; set; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValueResponse ToExchangeValueResponse (this ExchangeValue exchangeValue)
    {
        ExchangeValueResponse response = new ExchangeValueResponse()
        {
            UnitOfFirstValue = exchangeValue.UnitOfFirstValue,
            UnitOfSecondValue = exchangeValue.UnitOfSecondValue,
            FirstCurrencyId = exchangeValue.FirstCurrencyId,
            SecondCurrencyId = exchangeValue.SecondCurrencyId           
        };

        return response;
    }
}
