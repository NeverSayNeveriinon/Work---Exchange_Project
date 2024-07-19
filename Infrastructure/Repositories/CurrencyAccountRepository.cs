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
        
        List<CurrencyAccount> accountsList = await accounts.ToListAsync();
        
        return accountsList;
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByNumberAsync(string number)
    {
        CurrencyAccount? account = await _dbContext.CurrencyAccounts
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
    
    
    public CurrencyAccount UpdateCurrencyAccount(CurrencyAccount account, CurrencyAccount updatedCurrencyAccount)
    {
        _dbContext.Entry(account).Property(p => p.CurrencyID).IsModified = true;
        account.CurrencyID = updatedCurrencyAccount.CurrencyID;
        
        return account;
    }
    
    public CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal,decimal,decimal> calculationFunc)
    {
        _dbContext.Entry(account).Property(p => p.Balance).IsModified = true;
        var calculationResult = calculationFunc(account.Balance, moneyAmount);
        account.Balance = calculationResult;
        
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