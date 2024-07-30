using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

[Serializable]
public class TransactionResponse
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string FromAccountNumber { get; init; }
    public string? ToAccountNumber { get; init; }
    public decimal FromAccountChangeAmount { get; init; }
    public decimal ToAccountChangeAmount { get; init; } 

    public DateTime DateTime { get; init; }
    public string? RealTimeExchangeValue { get; init; }
    public TransactionStatusOptions TransactionStatus { get; init; }
    public TransactionTypeOptions TransactionType { get; init; }
    public decimal CRate { get; init; }
    // public decimal? FromAccountRemainingBalance { get; init; }
    public string? FromCurrencyType { get; init; }
    public string? ToCurrencyType { get; init; }
}


public static partial class TransactionExtensions
{
    public static TransactionResponse ToTransactionResponse (this Transaction transaction, decimal exchangeValue = 1, string? moneyCurrency = null)
    {
        var tranactionFromCurrencyType = transaction.FromAccount?.Currency?.CurrencyType;

        var tranactionToCurrencyType = transaction.ToAccount?.Currency?.CurrencyType;

        string realTimeExchangeValue = string.Empty;
        // decimal? fromAccountRemainingBalance = null;
        if (transaction.TransactionType == TransactionTypeOptions.Transfer)
        {
            realTimeExchangeValue = "1 " + tranactionFromCurrencyType + " = " + exchangeValue + " " + tranactionToCurrencyType;
            // fromAccountRemainingBalance = transaction.FromAccount?.Balance - transaction.FromAccountChangeAmount;
        }
        
        else if (transaction.TransactionType == TransactionTypeOptions.Deposit)
        {
            realTimeExchangeValue = "1 " + (moneyCurrency ?? tranactionFromCurrencyType) + " = " + exchangeValue + " " + tranactionFromCurrencyType;
            // fromAccountRemainingBalance = transaction.FromAccount?.Balance + transaction.FromAccountChangeAmount;
        }
        
        
        
        TransactionResponse response = new TransactionResponse()
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            CRate = transaction.CRate,
            FromAccountNumber = transaction.FromAccountNumber,
            ToAccountNumber = transaction.ToAccountNumber,
            FromAccountChangeAmount = transaction.FromAccountChangeAmount,
            ToAccountChangeAmount = transaction.ToAccountChangeAmount,
            FromCurrencyType =  tranactionFromCurrencyType,
            ToCurrencyType =  tranactionToCurrencyType,
            DateTime = transaction.DateTime,
            TransactionStatus = transaction.TransactionStatus,
            TransactionType = transaction.TransactionType,
            RealTimeExchangeValue = realTimeExchangeValue,
            // FromAccountRemainingBalance = fromAccountRemainingBalance
            // RealTimeExchangeValue = "1 " + tranactionFromCurrencyType + " = " + transaction.FromAccount.Currency.FirstExchangeValues.FirstOrDefault(exchangeValue => exchangeValue.SecondCurrencyId == transaction.ToAccount.CurrencyID)
            //                                     .UnitOfFirstValue + " " + tranactionToCurrencyType
        };

        return response;
    }
} 