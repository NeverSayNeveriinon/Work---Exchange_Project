using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.Domain.Entities;

public class Currency
{
    [Key]
    public int Id { get; set; }
    public CurrencyTypeOptions CurrencyType  { get; set; }
    
    
    // Relations //
    #region Relations
    
    // With "Currency(As SecondCurrency)" ---> FirstCurrency 'N'====......===='N' SecondCurrency -> in 'ExchangeRate' Entity
    public List<Currency>? SecondCurrencies { get; } = new List<Currency>(); // Navigation to 'Currency(As SecondCurrency)' entity
    public List<ExchangeRate>? FirstExchangeRates { get; } = new List<ExchangeRate>(); // Navigation to 'ExchangeRate(Join Entity)' entity
    
    // With "Currency(As FirstCurrency)" ---> SecondCurrency 'N'====......===='N' FirstCurrency -> in 'ExchangeRate' Entity
    public List<Currency>? FirstCurrencies { get; } = new List<Currency>(); // Navigation to 'Currency(As FirstCurrency)' entity
    public List<ExchangeRate>? SecondExchangeRates { get; } = new List<ExchangeRate>(); // Navigation to 'ExchangeRate(Join Entity)' entity
    
    #endregion
}