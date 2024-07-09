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
   

    public async Task<List<CurrencyAccount>> GetAllCurrencyAccounts()
    {
        var accounts = _dbContext.CurrencyAccounts.Include(property => property.Owner)
                                                  .Include(property => property.Currency)
                                                  .AsNoTracking();
        
        List<CurrencyAccount> accountsList = await accounts.ToListAsync();
        
        return accountsList;
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByID(int id)
    {
        CurrencyAccount? account = await _dbContext.CurrencyAccounts
                                              .Include(property => property.Owner)
                                              .Include(property => property.Currency)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(accountItem => accountItem.Number == id);

        return account;
    }

     
    public async Task<CurrencyAccount> AddCurrencyAccount(CurrencyAccount account)
    {
        var currencyAccountReturned = await _dbContext.CurrencyAccounts.AddAsync(account);

        return currencyAccountReturned.Entity;
    }
    
    public async Task<CurrencyAccount> UpdateCurrencyAccount(CurrencyAccount account, CurrencyAccount updatedCurrencyAccount)
    {
        _dbContext.Entry(account).Property(p => p.CurrencyID).IsModified = true;
        account.CurrencyID = updatedCurrencyAccount.CurrencyID;
        
        return account;
    }
    
    // public async Task<CurrencyAccount> UpdateDefinedAccount(CurrencyAccount account, int definedAccount)
    // {
    //     _dbContext.Entry(account).Property(p => p.CurrencyID).IsModified = true;
    //     account.CurrencyID = updatedCurrencyAccount.CurrencyID;
    //     
    //     return account;
    // }
    
    public async Task<bool> DeleteCurrencyAccount(CurrencyAccount account)
    {
        var entityEntry = _dbContext.CurrencyAccounts.Remove(account);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}