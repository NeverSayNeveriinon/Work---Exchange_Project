using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.IdentityEntities;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(UserProfileId),nameof(CurrencyAccountNumber), IsUnique = true)]
public class DefinedAccount
{
    [Key] 
    public Guid Id { get; set; }
    
    [ForeignKey("UserProfile")]
    public Guid UserProfileId { get; set; }
    
    [ForeignKey("CurrencyAccount")]
    [Column(TypeName="varchar(10)")]
    public string CurrencyAccountNumber { get; set; }
    
    public UserProfile? UserProfile { get; } = null!;
    public CurrencyAccount? CurrencyAccount { get; } = null!;
}