namespace Core.Helpers;

public static class CacheKeys
{
    public const string AllCommissionRatesKey = "AllCommissionRates";
    public static string GetCommissionRateKey(int commissionRateId) => $"commissionRate{commissionRateId}";
    
    
}