using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.Domain.Entities;

public class Currency
{
    [Key]
    public int Id { get; set; }
    public CurrencyTypeOptions CurrencyType  { get; set; }
    public Dictionary<int, decimal>? ExchangeRates { get; set; } = new Dictionary<int, decimal>();
}