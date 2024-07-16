using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }
    [Column(TypeName="money")]
    public decimal Amount { get; set; }
    public bool IsSuccess { get; set; }
    
    
    
    [ForeignKey("FromAccount")]
    public int FromAccountId { get; set; }
    
    [ForeignKey("ToAccount")]
    public int ToAccountId { get; set; }
    
    public CurrencyAccount FromAccount { get; } = null!;
    public CurrencyAccount ToAccount { get; } = null!;
}