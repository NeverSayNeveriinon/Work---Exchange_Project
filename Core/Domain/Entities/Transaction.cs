using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;
using Core.Helpers;

namespace Core.Domain.Entities;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'MaxUSDRange' Must Be Positive")]
    public decimal Amount { get; set; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'MaxUSDRange' Must Be Positive")]
    public decimal FromAccountChangeAmount { get; set; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'MaxUSDRange' Must Be Positive")]
    public decimal ToAccountChangeAmount { get; set; } 
    
    public DateTime DateTime { get; set; }
    public TransactionStatusOptions TransactionStatus { get; set; }
    public TransactionTypeOptions TransactionType { get; set; }    
    
    [Column(TypeName="decimal(6,5)")]
    [DecimalRange("0","0.5")]
    public decimal CRate { get; set; }    
    
    [ForeignKey("FromAccount")]
    [Column(TypeName="varchar(10)")]
    public string FromAccountNumber { get; set; }
    
    [ForeignKey("ToAccount")]
    [Column(TypeName="varchar(10)")]
    public string? ToAccountNumber { get; set; }
    
    public virtual CurrencyAccount? FromAccount { get; set; } = null!;
    public virtual CurrencyAccount? ToAccount { get; set; } = null!;
}