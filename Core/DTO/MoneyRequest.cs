using Core.Enums;

namespace Core.DTO;

public class MoneyRequest
{
    public decimal Amount { get; set; }
    public string CurrencyType { get; set; }
}