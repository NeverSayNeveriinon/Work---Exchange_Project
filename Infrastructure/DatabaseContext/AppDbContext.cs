using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Infrastructure.DatabaseContext;

public class AppDbContext : IdentityDbContext<UserProfile,UserRole,Guid>
{
    // public virtual DbSet<UserProfile> UserProfiles { get; set; }
    public virtual DbSet<CurrencyAccount> CurrencyAccounts { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<ExchangeValue> ExchangeValues { get; set; }
    public virtual DbSet<CommissionRate> CommissionRates { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // modelBuilder.Entity<Currency>()
        //     .Property(entity => entity.ExchangeValues)
        //     .HasConversion(
        //         v => JsonConvert.SerializeObject(v),
        //         v => JsonConvert.DeserializeObject<Dictionary<int, decimal>>(v));
        
        modelBuilder.Entity<UserProfile>()
            .Property(entity => entity.DefinedAccountNumbers)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v1 => JsonConvert.DeserializeObject<List<int>>(v1)!);

        modelBuilder.Entity<UserProfile>()
            .Ignore(entity => entity.PhoneNumber)
            .Ignore(entity => entity.PhoneNumberConfirmed);
        
        
        modelBuilder.Entity<CommissionRate>()
            .Property(entity => entity.CRate)
            .HasPrecision(6,3);
        
        // modelBuilder.Entity<CommissionRateRepository>()
        //     .Property(entity => entity.MaxUSDRange)
        //     .HasPrecision(20,3);
        
        
        
        // - Relationships
        
        // CurrencyAccount 'N'====......----'1' UserProfile
        modelBuilder.Entity<CurrencyAccount>()
            .HasOne(e => e.Owner)
            .WithMany(e => e.CurrencyAccounts)
            .HasForeignKey(e => e.OwnerID).IsRequired().OnDelete(DeleteBehavior.Cascade);    
        
        // CurrencyAccount 'N'====......----'1' Currency
        modelBuilder.Entity<CurrencyAccount>()
            .HasOne(e => e.Currency)
            .WithMany(e => e.CurrencyAccounts)
            .HasForeignKey(e => e.CurrencyID).IsRequired().OnDelete(DeleteBehavior.Cascade);
        
        // CurrencyAccount(as FromCurrencyAccount) 'N'----......----'N' CurrencyAccount(as ToCurrencyAccount)
        modelBuilder.Entity<CurrencyAccount>()
            .HasMany(e => e.ToCurrencyAccounts)
            .WithMany(e => e.FromCurrencyAccounts)
            .UsingEntity<Transaction>(
                l => l.HasOne<CurrencyAccount>(e => e.ToAccount)
                    .WithMany(e => e.ToTransactions)
                    .HasForeignKey(e => e.ToAccountId).IsRequired(false).OnDelete(DeleteBehavior.ClientCascade),
                r => r.HasOne<CurrencyAccount>(e => e.FromAccount)
                    .WithMany(e => e.FromTransactions)
                    .HasForeignKey(e => e.FromAccountId).IsRequired(false).OnDelete(DeleteBehavior.Cascade));
        
        // CurrencyAccount(as FirstCurrency) 'N'----......----'N' Currency(as SecondCurrency)
        modelBuilder.Entity<Currency>()
            .HasMany(e => e.SecondCurrencies)
            .WithMany(e => e.FirstCurrencies)
            .UsingEntity<ExchangeValue>(
                l => l.HasOne<Currency>(e => e.SecondCurrency)
                    .WithMany(e => e.SecondExchangeValues)
                    .HasForeignKey(e => e.SecondCurrencyId).IsRequired(false).OnDelete(DeleteBehavior.ClientCascade),
                r => r.HasOne<Currency>(e => e.FirstCurrency)
                    .WithMany(e => e.FirstExchangeValues)
                    .HasForeignKey(e => e.FirstCurrencyId).IsRequired(false).OnDelete(DeleteBehavior.Cascade));

    }
}