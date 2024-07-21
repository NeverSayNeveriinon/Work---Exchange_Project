using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Core.Domain.IdentityEntities;

public class UserProfile : IdentityUser<Guid>
{
    [StringLength(30)]
    public string? PersonName { get; init; }
    
    
    #region Relations
    
    //                              (Dependent)                        (Principal)
    // With "CurrencyAccount" ---> CurrencyAccount 'N'====......----'1' UserProfile
    public ICollection<CurrencyAccount>? CurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount' entity
    
    // With "CurrencyAccount" ---> UserProfile 'N'----......----'N' CurrencyAccount -> in 'DefinedAccount' Entity
    public ICollection<CurrencyAccount>? DefinedCurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount' entity
    public ICollection<DefinedAccount>? DefinedAccountsJoin { get; } = new List<DefinedAccount>(); // Navigation to 'DefinedAccount(Join Entity)' entity
    
    
    #endregion
    
}