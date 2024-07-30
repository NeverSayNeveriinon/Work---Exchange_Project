using Core.Domain.Entities;

namespace Core.DTO.ServicesDTO.Money;

public class Money
{
    public decimal Amount { get; set; }
    public Currency Currency { get; init; }
}