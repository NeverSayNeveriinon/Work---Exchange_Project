﻿using Core.Domain.Entities;
using Core.Helpers;

namespace Core.DTO.ExchangeValueDTO;

public class ExchangeValueUpdateRequest
{
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'UnitOfFirstValue' Must Be Positive")]
    public decimal UnitOfFirstValue { get; set; }
}

public static partial class ExchangeValueExtensions
{
    public static ExchangeValue ToExchangeValue(this ExchangeValueUpdateRequest exchangeValueAddRequest)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = exchangeValueAddRequest.UnitOfFirstValue
        };

        return exchangeValue;
    }
    public static ExchangeValue ToOppositeExchangeValue(this ExchangeValueUpdateRequest exchangeValueAddRequest)
    {
        ExchangeValue exchangeValue = new ExchangeValue()
        {
            UnitOfFirstValue = 1M / exchangeValueAddRequest.UnitOfFirstValue
        };

        return exchangeValue;
    }
}