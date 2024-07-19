namespace Core.Helpers;

public interface IValidator
{
    public Task<bool> ExistsInCurrentCurrencies(string currencyType);
}