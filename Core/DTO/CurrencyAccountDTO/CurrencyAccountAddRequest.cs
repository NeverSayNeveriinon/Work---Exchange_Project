using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountAddRequest
{
    public string CurrencyType { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountAddRequest currencyAccountAddRequest, Guid? OwnerID)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Number = 0,
            Balance = 0,
            Currency = new Currency(){CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), currencyAccountAddRequest.CurrencyType)},
            OwnerID = OwnerID.Value,
            DateTimeOfOpen = DateTime.Now
        };

        return currencyAccount;
    }
}