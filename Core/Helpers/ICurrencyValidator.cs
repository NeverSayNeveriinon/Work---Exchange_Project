namespace Core.Helpers;

public interface ICurrencyValidator
{
    Task<bool> ExistsInCurrentCurrencies(string currencyType);
}