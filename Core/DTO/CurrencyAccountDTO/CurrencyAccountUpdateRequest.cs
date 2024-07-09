using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountUpdateRequest
{
    public int CurrencyId { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountUpdateRequest currencyAccountUpdateRequest)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Number = 0,
            Balance = 0,
            // Currency = new Currency(){CurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), CurrencyAccountUpdateRequest.CurrencyType)},
            CurrencyID = currencyAccountUpdateRequest.CurrencyId,
            // Currency = new Currency(){Id = CurrencyAccountUpdateRequest.CurrencyId},
        };

        return currencyAccount;
    }
}