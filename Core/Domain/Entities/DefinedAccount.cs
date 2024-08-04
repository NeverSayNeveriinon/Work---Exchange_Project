using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.IdentityEntities;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(UserProfileId),nameof(CurrencyAccountNumber), IsUnique = true)]
public class DefinedAccount
{
    [Key] 
    public Guid Id { get; init; }
    
    [ForeignKey("UserProfile")]
    public Guid UserProfileId { get; init; }
    
    [ForeignKey("CurrencyAccount")]
    [Column(TypeName="char(10)")]
    public string CurrencyAccountNumber { get; init; }
    
    public UserProfile? UserProfile { get; } = null!;
    public CurrencyAccount? CurrencyAccount { get; } = null!;
}