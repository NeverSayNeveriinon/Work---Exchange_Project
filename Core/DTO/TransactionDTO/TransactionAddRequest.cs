using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionAddRequest
{
    public decimal Amount { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = 0,
            Amount = transactionAddRequest.Amount,
            FromAccountId = transactionAddRequest.FromAccountId,
            ToAccountId = transactionAddRequest.ToAccountId
        };

        return transaction;
    }
}