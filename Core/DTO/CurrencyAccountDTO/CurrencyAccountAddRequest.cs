using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountAddRequest
{
    [AllowedValues("USD","Euro","Rial", ErrorMessage = "The Value Should be one of these 'USD,Euro,Rial' ")]
    public string CurrencyType { get; set; }
    public MoneyOpenAccountRequest moneyToOpenAccount { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountAddRequest currencyAccountAddRequest, Guid? ownerID, int? currencyId)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Number = 0,
            Balance = 0,
            CurrencyID = currencyId.Value,
            OwnerID = ownerID.Value,
            DateTimeOfOpen = DateTime.Now
        };

        return currencyAccount;
    }
}