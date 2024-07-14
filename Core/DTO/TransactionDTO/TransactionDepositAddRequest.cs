using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionDepositAddRequest
{
    public MoneyRequest money { get; set; }
    public int AccountNumber { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionDepositAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = 0,
            Amount = 0,
            FromAccountNumber = transactionAddRequest.AccountNumber,
            ToAccountNumber = null,
            DateTime = DateTime.Now,
            TransactionType = TransactionTypeOptions.Deposit
        };

        return transaction;
    }
}