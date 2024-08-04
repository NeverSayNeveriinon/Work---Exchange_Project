using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;
using Core.Helpers;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountAddRequest
{
    [Required(ErrorMessage = "The 'CurrencyType' Can't Be Blank!!!")]
    [RegularExpression("^[A-Z]{3}$")]
    public string CurrencyType { get; set; }
    
    public MoneyRequest MoneyToOpenAccount { get; set; }
}

public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccount ToCurrencyAccount(this CurrencyAccountAddRequest currencyAccountAddRequest, Guid ownerID, int currencyId)
    {
        CurrencyAccount currencyAccount = new CurrencyAccount()
        {
            Number = Generator.RandomString(),
            Balance = 0,
            CurrencyID = currencyId,
            OwnerID = ownerID,
            DateTimeOfOpen = DateTime.Now
        };

        return currencyAccount;
    }
}