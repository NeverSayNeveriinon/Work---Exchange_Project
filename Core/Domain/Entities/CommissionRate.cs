using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities;

public class CommissionRate
{
    [Key]
    public int Id { get; set; }

    public decimal MaxUSDRange { get; set; }
    public decimal CRate { get; set; }
}