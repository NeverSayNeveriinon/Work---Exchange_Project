using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountRequest
{
    public string CuurrencyType { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountRequest currencyAccountRequest)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Number = 0,
            Balance = 0,
            Currency = new Currency(){CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), currencyAccountRequest.CuurrencyType)}
        };

        return currencyAccount;
    }
}