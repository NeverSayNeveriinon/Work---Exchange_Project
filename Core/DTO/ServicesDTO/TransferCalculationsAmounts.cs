namespace Core.DTO.ServicesDTO;

public class TransferCalculationsAmounts
{
    public decimal CRate { get; init; }
    public decimal CommissionAmount { get; init; }
    public decimal DestinationAmount { get; init; }
    public decimal ValueToBeMultiplied { get; init; }
}

public static class TransferCalculationsAmountsExtensions
{
    public static (decimal cRate, decimal commissionAmount, decimal destinationAmount, decimal valueToBeMultiplied) 
                   ToTuple(this TransferCalculationsAmounts calculationsAmounts)
    {
        return (calculationsAmounts.CRate, calculationsAmounts.CommissionAmount,
                calculationsAmounts.DestinationAmount, calculationsAmounts.ValueToBeMultiplied);
    }
}