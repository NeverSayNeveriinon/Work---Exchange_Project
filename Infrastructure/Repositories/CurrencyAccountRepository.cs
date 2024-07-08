using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CurrencyAccountRepository : ICurrencyAccountRepository
{
    private readonly AppDbContext _dbContext;

    
    public CurrencyAccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
   

    public async Task<List<CurrencyAccount>> GetAllCurrencyAccounts()
    {
        var accounts = _dbContext.CurrencyAccounts.AsNoTracking();
        
        List<CurrencyAccount> accountsList = await accounts.ToListAsync();
        
        return accountsList;
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByID(int id)
    {
        CurrencyAccount? account = await _dbContext.CurrencyAccounts
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(accountItem => accountItem.Number == id);

        return account;
    }

     
    public async Task<CurrencyAccount> AddCurrencyAccount(CurrencyAccount account)
    {
        await _dbContext.CurrencyAccounts.AddAsync(account);

        return account;
    }
    
    public async Task<CurrencyAccount> UpdateCurrencyAccount(CurrencyAccount account, CurrencyAccount updatedCurrencyAccount)
    {
        _dbContext.Entry(account).State = EntityState.Modified;

        _dbContext.Entry(account).CurrentValues.SetValues(updatedCurrencyAccount);
        
        return account;
    }
    
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