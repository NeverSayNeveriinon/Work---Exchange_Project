using Core.Enums;

namespace Core.DTO;

public class Money
{
    public decimal Amount { get; set; }
    public CurrencyTypeOptions CurrencyType { get; set; }
}