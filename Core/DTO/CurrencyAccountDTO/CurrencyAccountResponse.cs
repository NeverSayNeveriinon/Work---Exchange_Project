using Core.Domain.Entities;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountResponse
{
    public int Number { get; set; }
    public Guid OwnerId { get; set; }
    public string FullName { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyType { get; set; }

}


public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccountResponse ToCurrencyAccountResponse (this CurrencyAccount currencyAccount)
    {
        CurrencyAccountResponse response = new CurrencyAccountResponse()
        {
            Number = currencyAccount.Number,
            OwnerId = currencyAccount.OwnerID,
            FullName = currencyAccount.Owner.Name,
            Balance = currencyAccount.Balance,
            CurrencyType = nameof(currencyAccount.Currency.CurrencyType)
        };

        return response;
    }
}