using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Core.Domain.IdentityEntities;

public class UserProfile : IdentityUser<Guid>
{
    [StringLength(30)]
    public string? PersonName { get; set; }
    
    [StringLength(500)]
    public List<string>? DefinedAccountNumbers { get; set; } = new List<string>();
    
    // Relations //
    #region Relations
    
    //                              (Dependent)                        (Principal)
    // With "CurrencyAccount" ---> CurrencyAccount 'N'====......----'1' UserProfile
    public ICollection<CurrencyAccount>? CurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount' entity
    
    #endregion
    
}