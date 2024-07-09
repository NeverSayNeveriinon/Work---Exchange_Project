using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities;

public class CommissionRate
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName="money")]
    public decimal MaxUSDRange { get; set; }
    
    public double CRate { get; set; }
}