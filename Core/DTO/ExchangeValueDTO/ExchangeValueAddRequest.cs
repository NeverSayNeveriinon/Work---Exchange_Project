using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Enums;
using Core.Helpers;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueAddRequest
{
    [Required(ErrorMessage = "The 'FirstCurrencyType' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string FirstCurrencyType { get; set; }
    
    [Required(ErrorMessage = "The 'SecondCurrencyType' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string SecondCurrencyType { get; set; }
    
    [Required(ErrorMessage = "The 'UnitOfFirstValue' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue, ErrorMessage = "The 'UnitOfFirstValue' Must Be Positive")]
    public decimal? UnitOfFirstValue { get; set; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValue ToExchangeValue(this ExchangeValueAddRequest exchangeValueAddRequest, int firstCurrencyId, int secondCurrencyId)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = exchangeValueAddRequest.UnitOfFirstValue!.Value,
            FirstCurrencyId = firstCurrencyId,
            SecondCurrencyId = secondCurrencyId
        };

        return exchangeValue;
    }
    
    public static ExchangeValue ToOppositeExchangeValue(this ExchangeValueAddRequest exchangeValueAddRequest, int firstCurrencyId, int secondCurrencyId)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = 1M / exchangeValueAddRequest.UnitOfFirstValue!.Value,
            FirstCurrencyId = secondCurrencyId,
            SecondCurrencyId = firstCurrencyId
        };

        return exchangeValue;
    }
}