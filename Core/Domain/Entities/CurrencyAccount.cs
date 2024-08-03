using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.IdentityEntities;
using Core.Helpers;

namespace Core.Domain.Entities;

public class CurrencyAccount
{
    [Key]
    [Length(10,10)]  
    [Column(TypeName="varchar(10)")]
    public string Number { get; init; }
    
    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalRange.MaxValue)]
    public decimal Balance { get; set; }
    
    [Column(TypeName="decimal(20,9)")]
    public decimal StashBalance { get; set; }

    public DateTime DateTimeOfOpen { get; init; }
    
    // Relations //
    #region Relations
    
    //                          (Dependent)                        (Principal)
    // With "UserProfile" ---> CurrencyAccount 'N'====......----'1' UserProfile
    [ForeignKey("Owner")]
    public Guid OwnerID { get; set; }  // Foreign Key to 'UserProfile.UserId'
    public UserProfile? Owner { get; set; } = null!;  // Navigation to 'UserProfile' entity
    
    
    //                      (Dependent)                         (Principal)
    // With "Currency" ---> CurrencyAccount 'N'====......----'1' Currency
    [ForeignKey("Currency")]
    public int CurrencyID { get; set; }  // Foreign Key to 'Currency.UserId'
    public Currency? Currency { get; set; } = null!;  // Navigation to 'Currency' entity
        
    
    
    // With "CurrencyAccount(As ToCurrencyAccount)" ---> FromCurrencyAccount 'N'----......----'N' ToCurrencyAccount -> in 'Transaction' Entity
    public List<CurrencyAccount>? ToCurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount(As ToCurrencyAccount)' entity
    public List<Transaction>? FromTransactions { get; } = new List<Transaction>(); // Navigation to 'Transaction(Join Entity)' entity
    
    // With "CurrencyAccount(As FromCurrencyAccount)" ---> ToCurrencyAccount 'N'----......----'N' FromCurrencyAccount -> in 'Transaction' Entity
    public List<CurrencyAccount>? FromCurrencyAccounts { get; } = new List<CurrencyAccount>(); // Navigation to 'CurrencyAccount(As FromCurrencyAccount)' entity
    public List<Transaction>? ToTransactions { get; } = new List<Transaction>(); // Navigation to 'ExchangeValue(Join Entity)' entity
    
    
    // With "UserProfile(Defined)" ---> CurrencyAccount 'N'----......----'N' UserProfile -> in 'DefinedAccount' Entity
    public List<UserProfile>? DefinedUserProfiles { get; } = new List<UserProfile>(); // Navigation to 'UserProfile' entity
    public List<DefinedAccount>? DefinedAccountsJoin { get; } = new List<DefinedAccount>(); // Navigation to 'DefinedAccount(Join Entity)' entity

    #endregion
}