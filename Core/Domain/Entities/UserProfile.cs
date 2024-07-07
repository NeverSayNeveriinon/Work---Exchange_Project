using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities;

public class UserProfile
{
    [Key]
    public Guid UserId { get; set; }
    public List<DefinedAccount> DefinedAccounts { get; set; } = new List<DefinedAccount>();
}