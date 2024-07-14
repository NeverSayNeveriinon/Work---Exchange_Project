using Core.Enums;

namespace Core.DTO;

public class MoneyOpenAccountRequest
{
    public decimal Amount { get; set; }
    public string CurrencyType { get; set; }
}