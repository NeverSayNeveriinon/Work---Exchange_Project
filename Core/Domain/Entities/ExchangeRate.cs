using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities;

public class ExchangeRate
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("FirstCurrency")]
    public int FirstCurrencyId { get; set; }
    
    [ForeignKey("SecondCurrency")]
    public int SecondCurrencyId { get; set; }
    
    public Currency FirstCurrency { get; } = null!;
    public Currency SecondCurrency { get; } = null!;
    
    public decimal UnitOfFirstRate { get; set; }
    public decimal UnitOfSecondRate { get; set; }
    
}