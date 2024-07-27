using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;
using Core.Helpers;

namespace Core.DTO.TransactionDTO;

public class TransactionTransferAddRequest
{
    [Required(ErrorMessage = "The 'Amount' Can't Be Blank!!!")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'Amount' Must Be Positive")]
    public decimal? Amount { get; set; }
    
    [Required(ErrorMessage = "The 'FromAccountNumber' Can't Be Blank!!!")]
    [Length(10,10, ErrorMessage = "The 'AccountNumber' is Not In a Correct Format")]  
    public string FromAccountNumber { get; set; }
    
    [Required(ErrorMessage = "The 'ToAccountNumber' Can't Be Blank!!!")]
    [Length(10,10, ErrorMessage = "The 'AccountNumber' is Not In a Correct Format")]  
    public string ToAccountNumber { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionTransferAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = Guid.NewGuid(),
            Amount = transactionAddRequest.Amount.GetValueOrDefault(),
            FromAccountNumber = transactionAddRequest.FromAccountNumber,
            ToAccountNumber = transactionAddRequest.ToAccountNumber,
            DateTime = DateTime.Now,
            TransactionType = TransactionTypeOptions.Transfer,
            CRate = 0,
            TransactionStatus = TransactionStatusOptions.Pending
        };

        return transaction;
    }
}