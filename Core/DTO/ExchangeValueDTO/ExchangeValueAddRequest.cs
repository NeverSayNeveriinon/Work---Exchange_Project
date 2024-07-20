using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueAddRequest
{
    [Required(ErrorMessage = "The 'FirstCurrencyType' Can't Be Blank!!!")]
    public string FirstCurrencyType { get; set; }
    
    [Required(ErrorMessage = "The 'SecondCurrencyType' Can't Be Blank!!!")]
    public string SecondCurrencyType { get; set; }
    
    [Required(ErrorMessage = "The 'UnitOfFirstValue' Can't Be Blank!!!")]
    public decimal? UnitOfFirstValue { get; set; }
    
    [Required(ErrorMessage = "The 'UnitOfSecondValue' Can't Be Blank!!!")]
    public decimal? UnitOfSecondValue { get; set; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValue ToExchangeValue(this ExchangeValueAddRequest exchangeValueAddRequest, int firstCurrencyId, int secondCurrencyId)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = exchangeValueAddRequest.UnitOfFirstValue!.Value,
            UnitOfSecondValue = exchangeValueAddRequest.UnitOfSecondValue!.Value,
            FirstCurrencyId = firstCurrencyId,
            SecondCurrencyId = secondCurrencyId
        };

        return exchangeValue;
    }
}