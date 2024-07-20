using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.Enums;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _dbContext;

    
    public CurrencyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
   

    public async Task<List<Currency>> GetAllCurrenciesAsync()
    {
        var currenciesList = await _dbContext.Currencies.AsNoTracking()
                                                        .ToListAsync();
        
        return currenciesList;
    }

    public async Task<Currency?> GetCurrencyByIDAsync(int id)
    {
        var currency = await _dbContext.Currencies.AsNoTracking()
                                                  .FirstOrDefaultAsync(currencyItem => currencyItem.Id == id);

        return currency;
    }
    
    public async Task<Currency?> GetCurrencyByCurrencyTypeAsync(CurrencyTypeOptions currencyType)
    {
        var currency = await _dbContext.Currencies.AsNoTracking()
                                                  .FirstOrDefaultAsync(currencyItem => currencyItem.CurrencyType == currencyType);

        return currency;
    }

     
    public async Task<Currency> AddCurrencyAsync(Currency currency)
    {
        var currencyReturned = await _dbContext.Currencies.AddAsync(currency);

        return currencyReturned.Entity;
    }
    
    
    public bool DeleteCurrency(Currency currency)
    {
        var entityEntry = _dbContext.Currencies.Remove(currency);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}