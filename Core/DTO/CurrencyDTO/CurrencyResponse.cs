using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyDTO;

public class CurrencyResponse
{
    public int Id { get; set; }
    public string CurrencyType  { get; set; }
}

public static partial class CurrencyExtensions
{
    public static CurrencyResponse ToCurrencyResponse (this Currency currency)
    {
        CurrencyResponse response = new CurrencyResponse()
        {
            Id = currency.Id,
            CurrencyType = Enum.GetName(typeof (CurrencyTypeOptions), currency.CurrencyType)
        };

        return response;
    }
}