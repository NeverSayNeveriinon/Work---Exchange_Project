using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int FromAccountNumber { get; set; }
    public int ToAccountNumber { get; set; }

    public DateTime DateTime { get; set; }
    public string? RealTimeExchangeValue { get; set; }
    public double CommissionRate { get; set; }
    public string? FromCurrencyType { get; set; }
    public string? ToCurrencyType { get; set; }
}


public static partial class TransactionExtensions
{
    public static TransactionResponse ToTransactionResponse (this Transaction transaction)
    {
        var tranactionFromCurrencyType = transaction.FromAccount == null || transaction.FromAccount.Currency == null
            ? null :Enum.GetName(typeof(CurrencyTypeOptions), transaction.FromAccount.Currency.CurrencyType);
        
        var tranactionToCurrencyType = transaction.ToAccount == null || transaction.ToAccount.Currency == null
            ? null :Enum.GetName(typeof(CurrencyTypeOptions), transaction.ToAccount.Currency.CurrencyType);

        TransactionResponse response = new TransactionResponse()
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            FromAccountNumber = transaction.FromAccountNumber,
            ToAccountNumber = transaction.ToAccountNumber,
            FromCurrencyType =  tranactionFromCurrencyType,
            ToCurrencyType =  tranactionToCurrencyType,
            DateTime = transaction.DateTime,
            RealTimeExchangeValue = "1 " + tranactionFromCurrencyType + " = " + (transaction.FromAccount == null || transaction.FromAccount.Currency == null
                                    ? null : transaction.FromAccount.Currency.FirstExchangeValues.FirstOrDefault(exchangeValue => exchangeValue.SecondCurrencyId == transaction.ToAccount.CurrencyID)
                                                .UnitOfFirstValue.ToString()) + " " + tranactionToCurrencyType
        };

        return response;
    }
} 