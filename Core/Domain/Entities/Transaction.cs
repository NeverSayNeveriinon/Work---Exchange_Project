using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using Core.Enums;

namespace Core.Domain.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }
    [Column(TypeName="money")]
    public decimal Amount { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime DateTime { get; set; }
    public TransactionTypeOptions TransactionType { get; set; }    
    public decimal CRate { get; set; }    
    
    [ForeignKey("FromAccount")]
    public int FromAccountNumber { get; set; }
    
    [ForeignKey("ToAccount")]
    public int? ToAccountNumber { get; set; }
    
    public CurrencyAccount? FromAccount { get; } = null!;
    public CurrencyAccount? ToAccount { get; } = null!;
}