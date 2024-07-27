using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(CurrencyType), IsUnique = true)]
public class Currency
{
    [Key]
    public int Id { get; set; }
    
    [Column(TypeName="varchar(3)")]
    public string CurrencyType  { get; set; }
    
    
    // Relations //
    #region Relations
    
    
    //                              (Dependent)                        (Principal)
    // With "CurrencyAccount" ---> CurrencyAccount 'N'====......----'1' Currency
    public ICollection<CurrencyAccount>? CurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount' entity
    
    
    // With "Currency(As SecondCurrency)" ---> FirstCurrency 'N'====......===='N' SecondCurrency -> in 'ExchangeValue' Entity
    public List<Currency>? SecondCurrencies { get; } = new List<Currency>(); // Navigation to 'Currency(As SecondCurrency)' entity
    public List<ExchangeValue>? FirstExchangeValues { get; } = new List<ExchangeValue>(); // Navigation to 'ExchangeValue(Join Entity)' entity
    
    // With "Currency(As FirstCurrency)" ---> SecondCurrency 'N'====......===='N' FirstCurrency -> in 'ExchangeValue' Entity
    public List<Currency>? FirstCurrencies { get; } = new List<Currency>(); // Navigation to 'Currency(As FirstCurrency)' entity
    public List<ExchangeValue>? SecondExchangeValues { get; } = new List<ExchangeValue>(); // Navigation to 'ExchangeValue(Join Entity)' entity
    
    #endregion
}