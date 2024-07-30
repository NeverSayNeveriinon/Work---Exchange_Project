using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO.Money;

public class Money
{
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
}