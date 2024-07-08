using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities;

public class UserProfile
{
    [Key] 
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    
    public List<DefinedAccount> DefinedAccounts { get; set; } = new List<DefinedAccount>();
    
    
    // Relations //
    #region Relations
    
    //                              (Dependent)                        (Principal)
    // With "CurrencyAccount" ---> CurrencyAccount 'N'====......----'1' UserProfile
    public ICollection<CurrencyAccount>? CurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount' entity
    
    #endregion
}