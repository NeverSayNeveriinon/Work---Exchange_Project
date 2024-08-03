using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;
using Core.Helpers;
using Core.Helpers.CustomValidateAttribute;

namespace Core.Domain.Entities;

public class Transaction
{
    [Key]
    public Guid Id { get; init; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue)]
    public decimal Amount { get; init; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue)]
    public decimal FromAccountChangeAmount { get; set; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue)]
    public decimal ToAccountChangeAmount { get; set; } 
    
    public DateTime DateTime { get; init; }
    public TransactionStatusOptions TransactionStatus { get; set; }
    public TransactionTypeOptions TransactionType { get; init; }    
    
    [Column(TypeName="decimal(6,5)")]
    [DecimalRange("0","0.5")]
    public decimal CRate { get; set; }    
    
    [ForeignKey("FromAccount")]
    [Column(TypeName="varchar(10)")]
    public string FromAccountNumber { get; init; }
    
    [ForeignKey("ToAccount")]
    [Column(TypeName="varchar(10)")]
    public string? ToAccountNumber { get; init; }
    
    public virtual CurrencyAccount? FromAccount { get; set; } = null!;
    public virtual CurrencyAccount? ToAccount { get; set; } = null!;
}