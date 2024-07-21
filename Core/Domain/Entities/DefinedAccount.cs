using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.IdentityEntities;

namespace Core.Domain.Entities;

public class DefinedAccount
{
    [Key] 
    public Guid Id { get; set; }
    
    [ForeignKey("UserProfile")]
    public Guid UserProfileId { get; set; }
    
    [ForeignKey("CurrencyAccount")]
    public string CurrencyAccountNumber { get; set; }
    
    public UserProfile? UserProfile { get; } = null!;
    public CurrencyAccount? CurrencyAccount { get; } = null!;
}