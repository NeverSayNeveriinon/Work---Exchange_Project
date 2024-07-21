using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionDepositAddRequest
{
    public MoneyRequest Money { get; set; }
    
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    public string AccountNumber { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionDepositAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = Guid.NewGuid(),
            Amount = 0,
            FromAccountNumber = transactionAddRequest.AccountNumber,
            ToAccountNumber = null,
            DateTime = DateTime.Now,
            TransactionType = TransactionTypeOptions.Deposit,
            IsConfirmed = false
        };

        return transaction;
    }
}