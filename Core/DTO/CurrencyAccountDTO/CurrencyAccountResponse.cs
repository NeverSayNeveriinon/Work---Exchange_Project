using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountResponse
{
    public string Number { get; init; }
    public Guid OwnerID { get; init; }
    public string? FullName { get; init; }
    public decimal Balance { get; init; }
    public string? CurrencyType { get; init; }
    public DateTime DateTimeOfOpen { get; init; }
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