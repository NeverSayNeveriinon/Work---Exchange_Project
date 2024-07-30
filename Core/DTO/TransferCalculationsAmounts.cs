namespace Core.DTO;

public class TransferCalculationsAmounts
{
    public decimal CRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal DestinationAmount { get; set; }
    public decimal ValueToBeMultiplied { get; set; }
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