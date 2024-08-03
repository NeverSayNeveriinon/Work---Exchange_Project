using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Helpers;
using Core.Helpers.CustomValidateAttribute;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(FirstCurrencyId),nameof(SecondCurrencyId), IsUnique = true)]
public class ExchangeValue
{
    [Key]
    public int Id { get; init; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue)]
    public decimal UnitOfFirstValue { get; set; }
    
    [ForeignKey("FirstCurrency")]
    public int FirstCurrencyId { get; init; }
    
    [ForeignKey("SecondCurrency")]
    public int SecondCurrencyId { get; init; }
    
    public Currency FirstCurrency { get; set; } = null!;
    public Currency SecondCurrency { get; set; } = null!;
}