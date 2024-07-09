using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities;

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
    
    public Currency FirstCurrency { get; } = null!;
    public Currency SecondCurrency { get; } = null!;
}