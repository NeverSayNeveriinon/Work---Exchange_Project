using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.DTO.ServicesDTO.Money;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionDepositAddRequest
{
    public MoneyRequest Money { get; set; }
    
    [Required(ErrorMessage = "The 'AccountNumber' Can't Be Blank!!!")]
    [Length(10,10, ErrorMessage = "The 'AccountNumber' is Not In a Correct Format")]  
    public string AccountNumber { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionDepositAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = Guid.NewGuid(),
            Amount = transactionAddRequest.Money.Amount.GetValueOrDefault(),
            FromAccountNumber = transactionAddRequest.AccountNumber,
            ToAccountNumber = null,
            DateTime = DateTime.Now,
            TransactionType = TransactionTypeOptions.Deposit,
            CRate = 0,
            TransactionStatus = TransactionStatusOptions.Pending
        };

        return transaction;
    }
}