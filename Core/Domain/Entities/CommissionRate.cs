using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(MaxUSDRange), IsUnique = true)]
public class CommissionRate
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName="money")]
    public decimal MaxUSDRange { get; set; }
    
    [Range(0,100)]
    [Column(TypeName="decimal")]
    public decimal CRate { get; set; }
}