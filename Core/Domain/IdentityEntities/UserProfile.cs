using System.ComponentModel.DataAnnotations;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Core.Domain.IdentityEntities;

public class UserProfile : IdentityUser<Guid>
{
    [StringLength(30, ErrorMessage = "The 'Person Name' Can't Be More Than 30 Characters")]
    public string? PersonName { get; set; }
    
    public List<int> DefinedAccountNumbers { get; set; } = new List<int>();
    
    // Relations //
    #region Relations
    
    //                              (Dependent)                        (Principal)
    // With "CurrencyAccount" ---> CurrencyAccount 'N'====......----'1' UserProfile
    public ICollection<CurrencyAccount>? CurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount' entity
    
    #endregion
    
}