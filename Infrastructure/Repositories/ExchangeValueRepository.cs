using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExchangeValueRepository : IExchangeValueRepository
{
    private readonly AppDbContext _dbContext;

    
    public ExchangeValueRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
   

    public async Task<List<ExchangeValue>> GetAllExchangeValues()
    {
        var currencies = _dbContext.ExchangeValues.AsNoTracking();
        
        List<ExchangeValue> currenciesList = await currencies.ToListAsync();
        
        return currenciesList;
    }

    public async Task<ExchangeValue?> GetExchangeValueByID(int id)
    {
        ExchangeValue? exchangeValue = await _dbContext.ExchangeValues
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(exchangeValueItem => exchangeValueItem.Id == id);

        return exchangeValue;
    }

     
    public async Task<ExchangeValue> AddExchangeValue(ExchangeValue exchangeValue)
    {
        var exchangeValueReturned = await _dbContext.ExchangeValues.AddAsync(exchangeValue);

        return exchangeValueReturned.Entity;
    }
    
    public async Task<ExchangeValue> UpdateExchangeValue(ExchangeValue exchangeValue, ExchangeValue updatedExchangeValue)
    {
        // _dbContext.Entry(exchangeValue).Property(p => p.ExchangeValueType).IsModified = true;
        // exchangeValue.ExchangeValueType = updatedExchangeValue.ExchangeValueType;

        return exchangeValue;
    }
    
    public async Task<bool> DeleteExchangeValue(ExchangeValue exchangeValue)
    {
        var entityEntry = _dbContext.ExchangeValues.Remove(exchangeValue);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}