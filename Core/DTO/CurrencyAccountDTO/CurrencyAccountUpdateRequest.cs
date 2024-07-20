using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountUpdateRequest
{
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
    public string CurrencyType { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountUpdateRequest currencyAccountUpdateRequest, int currencyId)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Balance = 0,
            CurrencyID = currencyId
        };

        return currencyAccount;
    }
}