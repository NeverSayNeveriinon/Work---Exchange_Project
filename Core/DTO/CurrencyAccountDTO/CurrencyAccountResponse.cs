using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountResponse
{
    public string Number { get; set; }
    public Guid OwnerID { get; set; }
    public string? FullName { get; set; }
    public decimal Balance { get; set; }
    public string? CurrencyType { get; set; }
    public DateTime DateTimeOfOpen { get; set; }
}


public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccountResponse ToCurrencyAccountResponse (this CurrencyAccount currencyAccount)
    {
        CurrencyAccountResponse response = new CurrencyAccountResponse()
        {
            Number = currencyAccount.Number,
            OwnerID = currencyAccount.OwnerID,
            FullName = currencyAccount.Owner?.PersonName,
            Balance = currencyAccount.Balance,
            CurrencyType = currencyAccount.Currency?.CurrencyType,
            DateTimeOfOpen = currencyAccount.DateTimeOfOpen
        };

        return response;
    }
}