using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Infrastructure.DatabaseContext;

public class AppDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,Guid>
{
    public virtual DbSet<UserProfile> UserProfiles { get; set; }
    public virtual DbSet<DefinedAccount> DefinedAccounts { get; set; }
    public virtual DbSet<CurrencyAccount> CurrencyAccounts { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Currency>()
            .Property(entity => entity.ExchangeRates)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<int, decimal>>(v));
    }
}