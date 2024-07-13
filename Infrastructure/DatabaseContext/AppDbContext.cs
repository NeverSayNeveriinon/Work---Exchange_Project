using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
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

        #region Role_Seed
            var adminRole = new UserRole()
            {
                Id = Guid.Parse("0D8EA822-1454-4853-9753-78FCDBD429D3"),
                Name = "Admin",
                NormalizedName = "Admin".ToUpper(),
                ConcurrencyStamp = Guid.Parse("878EEDAF-E795-411E-A0FC-847D0D4193DC").ToString()
            };
            var userRole = new UserRole()
            {
                Id = Guid.Parse("6C1FC012-261F-4E18-AAB5-0B4A685B2860"),
                Name = "User",
                NormalizedName = "User".ToUpper(),
                ConcurrencyStamp = Guid.Parse("7702FAFB-3813-46DB-9695-66A6E8FE9D41").ToString()
            };
            modelBuilder.Entity<UserRole>().HasData([adminRole,userRole]);
        #endregion

        #region AdminProfile_Seed
            PasswordHasher<UserProfile> ph = new PasswordHasher<UserProfile>();
            var adminProfile = new UserProfile()
            {
                Id = Guid.Parse("692DE906-FF0C-4ECB-9B79-9D94FC72DEAD"),
                PersonName = "Admin Admini",
                UserName = "admin@gmail.com",
                NormalizedUserName = "admin@gmail.com".ToUpper(),
                Email = "admin@gmail.com",
                NormalizedEmail = "admin@gmail.com".ToUpper(),
                EmailConfirmed = true,
                DefinedAccountNumbers = new List<int>(),
                PhoneNumber = "09991234567",
                ConcurrencyStamp = Guid.Parse("D594E4F9-C74E-43A4-90AC-F7FEC50C15E1").ToString(),
                PasswordHash = null
            };
            adminProfile.PasswordHash = ph.HashPassword(adminProfile, "adminpass");
            modelBuilder.Entity<UserProfile>().HasData(adminProfile);
            modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>{RoleId = adminRole.Id, UserId = adminProfile.Id});
        #endregion
        

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
                    .HasForeignKey(e => e.ToAccountNumber).IsRequired(false).OnDelete(DeleteBehavior.ClientCascade),
                r => r.HasOne<CurrencyAccount>(e => e.FromAccount)
                    .WithMany(e => e.FromTransactions)
                    .HasForeignKey(e => e.FromAccountNumber).IsRequired(false).OnDelete(DeleteBehavior.Cascade));
        
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