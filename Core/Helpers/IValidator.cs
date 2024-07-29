namespace Core.Helpers;

public interface IValidator
{
    Task<bool> ExistsInCurrentCurrencies(string currencyType);
}