using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
// using System.Data.Entity;

namespace Infrastructure.Repositories;

// TODO: Using Generic Repository
// todo: return Iqueryable from repository
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
                                                  .ThenInclude(property => property.FirstExchangeValues)
                                                  .ThenInclude(property => property.SecondCurrency);
        var accountsList = await accounts.ToListAsync();
        return accountsList;
    }
    
    // public IQueryable<CurrencyAccount> GetCurrencyAccounts()
    // {
    //     var accounts = _dbContext.CurrencyAccounts.Include(property => property.Owner)
    //                                                 .Include(property => property.Currency)
    //                                                 .ThenInclude(property => property.FirstExchangeValues)
    //                                                 .ThenInclude(property => property.SecondCurrency);
    //     // var accountsList = await accounts.ToListAsync();
    //     return accounts;
    // }
    
    public async Task<List<CurrencyAccount>> GetAllCurrencyAccountsByUserAsync(Guid ownerID)
    {
        var accounts = _dbContext.CurrencyAccounts.Include(property => property.Owner)
                                                  .Include(property => property.Currency)
                                                  .Where(property => property.OwnerID == ownerID);
        var accountsList = await accounts.ToListAsync();
        return accountsList;
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByNumberAsync(string number)
    {
        var account = await _dbContext.CurrencyAccounts.Include(property => property.Owner)
                                                       .Include(property => property.Currency)
                                                       .ThenInclude(property => property.FirstExchangeValues)
                                                       .Include(property => property.Currency)
                                                       .ThenInclude(property => property.SecondExchangeValues)
                                                       .ThenInclude(property => property.FirstCurrency)
                                                       .FirstOrDefaultAsync(accountItem => accountItem.Number == number);
        return account;
    }

     
    public async Task<CurrencyAccount> AddCurrencyAccountAsync(CurrencyAccount account)
    {
        var currencyAccountReturned = await _dbContext.CurrencyAccounts.AddAsync(account);

        return currencyAccountReturned.Entity;
    }
    
    public CurrencyAccount UpdateStashBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal,decimal,decimal> calculationFunc)
    {
        if (_dbContext.Entry(account).State == EntityState.Detached)
            _dbContext.Entry(account).Property(p => p.StashBalance).IsModified = true;
            
        account.StashBalance = calculationFunc(account.StashBalance, moneyAmount);
        return account;
    }

    public CurrencyAccount UpdateBalanceAmount(CurrencyAccount account, decimal moneyAmount, Func<decimal,decimal,decimal> calculationFunc)
    {
        if (_dbContext.Entry(account).State == EntityState.Detached)
            _dbContext.Entry(account).Property(p => p.Balance).IsModified = true;
 
        account.Balance = calculationFunc(account.Balance, moneyAmount);
        return account;
    }
    
    public void DeleteCurrencyAccount(CurrencyAccount account)
    {
        _dbContext.CurrencyAccounts.Remove(account);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}