using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// TODO: Using Generic Repository
public class CurrencyAccountRepository : ICurrencyAccountRepository
{
    private readonly AppDbContext _dbContext;
    
    public CurrencyAccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CurrencyAccount>> GetAllCurrencyAccountsAsync()
    {
        var accounts = _dbContext.CurrencyAccounts.Include(property => property.Owner)
                                                  .Include(property => property.Currency)
                                                  .AsNoTracking();
        
        var accountsList = await accounts.ToListAsync();
        return accountsList;
    }
    
    public async Task<List<CurrencyAccount>> GetAllCurrencyAccountsByUserAsync(Guid ownerID)
    {
        var accounts = _dbContext.CurrencyAccounts.Include(property => property.Owner)
                                                  .Include(property => property.Currency)
                                                  .Where(property => property.OwnerID == ownerID)
                                                  .AsNoTracking();
        
        var accountsList = await accounts.ToListAsync();
        return accountsList;
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByNumberAsync(string number)
    {
        var account = await _dbContext.CurrencyAccounts
                                              .Include(property => property.Owner)
                                              .Include(property => property.Currency)
                                              .ThenInclude(property => property.SecondExchangeValues)
                                              .ThenInclude(property => property.FirstCurrency)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(accountItem => accountItem.Number == number);

        return account;
    }

     
    public async Task<CurrencyAccount> AddCurrencyAccountAsync(CurrencyAccount account)
    {
        var currencyAccountReturned = await _dbContext.CurrencyAccounts.AddAsync(account);

        return currencyAccountReturned.Entity;
    }
    
    public CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal,decimal,decimal> calculationFunc)
    {
        _dbContext.Entry(account).Property(p => p.Balance).IsModified = true;
        account.Balance = calculationFunc(account.Balance, moneyAmount);
        
        return account;
    }
    
    public bool DeleteCurrencyAccount(CurrencyAccount account)
    {
        var entityEntry = _dbContext.CurrencyAccounts.Remove(account);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}