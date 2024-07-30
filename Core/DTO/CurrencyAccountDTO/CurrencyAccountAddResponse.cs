using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.CurrencyAccountDTO;

public class CurrencyAccountAddResponse : CurrencyAccountResponse
{
    public Guid TransactionId { get; init; }
}


public static partial class CurrencyAccountExtensions
{
    public static CurrencyAccountAddResponse ToCurrencyAccountResponse (this CurrencyAccount currencyAccount, Guid transactionId)
    {
        CurrencyAccountAddResponse response = new CurrencyAccountAddResponse()
        {
            Number = currencyAccount.Number,
            OwnerID = currencyAccount.OwnerID,
            FullName = currencyAccount.Owner?.PersonName,
            Balance = currencyAccount.Balance,
            TransactionId = transactionId,
            CurrencyType = currencyAccount.Currency?.CurrencyType,
            DateTimeOfOpen = currencyAccount.DateTimeOfOpen
        };

        return response;
    }
}