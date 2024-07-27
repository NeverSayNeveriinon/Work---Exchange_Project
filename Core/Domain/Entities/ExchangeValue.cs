using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(FirstCurrencyId),nameof(SecondCurrencyId), IsUnique = true)]
public class ExchangeValue
{
    [Key]
    public int Id { get; set; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'MaxUSDRange' Must Be Positive")]
    public decimal UnitOfFirstValue { get; set; }
    
    [ForeignKey("FirstCurrency")]
    public int FirstCurrencyId { get; set; }
    
    [ForeignKey("SecondCurrency")]
    public int SecondCurrencyId { get; set; }
    
    public Currency FirstCurrency { get; set; } = null!;
    public Currency SecondCurrency { get; set; } = null!;
}