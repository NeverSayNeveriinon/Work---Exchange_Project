using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(FirstCurrencyId),nameof(SecondCurrencyId), IsUnique = true)]
public class ExchangeValue
{
    [Key]
    public int Id { get; set; }
    
    [Column(TypeName="money")]
    public decimal UnitOfFirstValue { get; set; }
    [Column(TypeName="money")]
    public decimal UnitOfSecondValue { get; set; }
    
    
    [ForeignKey("FirstCurrency")]
    public int FirstCurrencyId { get; set; }
    
    [ForeignKey("SecondCurrency")]
    public int SecondCurrencyId { get; set; }
    
    public Currency FirstCurrency { get; set; } = null!;
    public Currency SecondCurrency { get; set; } = null!;
}