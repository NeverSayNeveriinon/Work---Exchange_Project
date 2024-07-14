using Core.Domain.Entities;
using Core.Enums;

namespace Core.DTO;

public class Money
{
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
}