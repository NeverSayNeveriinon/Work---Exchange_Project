using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountAddRequest
{
    public int CurrencyId { get; set; }
    public Guid OwnerID { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountAddRequest currencyAccountAddRequest)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Number = 0,
            Balance = 0,
            // Currency = new Currency(){CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), CurrencyAccountAddRequest.CurrencyType)},
            CurrencyID = currencyAccountAddRequest.CurrencyId,
            OwnerID = currencyAccountAddRequest.OwnerID
            // Currency = new Currency(){Id = CurrencyAccountAddRequest.CurrencyId},
            // Owner = new UserProfile(){Id = CurrencyAccountAddRequest.OwnerID}
        };

        return currencyAccount;
    }
}