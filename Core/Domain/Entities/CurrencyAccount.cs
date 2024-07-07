using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities;

public class CurrencyAccount
{
    [Key]
    public int Number { get; set; }
    public Guid OwnerId { get; set; }
    public decimal Balance { get; set; }
}