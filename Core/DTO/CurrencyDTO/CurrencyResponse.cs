using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyDTO;

public class CurrencyResponse
{
    public int Id { get; init; }
    public string CurrencyType  { get; init; }
}

public static partial class CurrencyExtensions
{
    public static CurrencyResponse ToCurrencyResponse (this Currency currency)
    {
        CurrencyResponse response = new CurrencyResponse()
        {
            Id = currency.Id,
            CurrencyType = currency.CurrencyType
        };

        return response;
    }
}