using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionTransferAddRequest
{
    [Required(ErrorMessage = "The 'Amount' Can't Be Blank!!!")]
    public decimal? Amount { get; set; }
    
    [Required(ErrorMessage = "The 'FromAccountNumber' Can't Be Blank!!!")]
    public string FromAccountNumber { get; set; }
    
    [Required(ErrorMessage = "The 'ToAccountNumber' Can't Be Blank!!!")]
    public string ToAccountNumber { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionTransferAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = 0,
            Amount = 0,
            FromAccountNumber = transactionAddRequest.FromAccountNumber,
            ToAccountNumber = transactionAddRequest.ToAccountNumber,
            DateTime = DateTime.Now,
            TransactionType = TransactionTypeOptions.Transfer,
            IsConfirmed = false
        };

        return transaction;
    }
}