using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
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
        var currencies = _dbContext.Currencies.AsNoTracking();
        
        List<Currency> currenciesList = await currencies.ToListAsync();
        
        return currenciesList;
    }

    public async Task<Currency?> GetCurrencyByIDAsync(int id)
    {
        Currency? currency = await _dbContext.Currencies
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(currencyItem => currencyItem.Id == id);

        return currency;
    }

     
    public async Task<Currency> AddCurrencyAsync(Currency currency)
    {
        var currencyReturned = await _dbContext.Currencies.AddAsync(currency);

        return currencyReturned.Entity;
    }
    
    public Currency UpdateCurrency(Currency currency, Currency updatedCurrency)
    {
        _dbContext.Entry(currency).Property(p => p.CurrencyType).IsModified = true;
        currency.CurrencyType = updatedCurrency.CurrencyType;

        return currency;
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