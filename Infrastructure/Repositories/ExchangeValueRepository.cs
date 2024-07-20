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
   

    public async Task<List<ExchangeValue>> GetAllExchangeValuesAsync()
    {
        var exchangeValuesList = await _dbContext.ExchangeValues.AsNoTracking()
                                                                .ToListAsync();
        return exchangeValuesList;
    }

    public async Task<ExchangeValue?> GetExchangeValueByIDAsync(int id)
    {
        var exchangeValue = await _dbContext.ExchangeValues.AsNoTracking()
                                                           .FirstOrDefaultAsync(exchangeValueItem => exchangeValueItem.Id == id);
        return exchangeValue;
    }
    
    public async Task<ExchangeValue?> GetExchangeValueByCurrenciesIDAsync(int firstCurrencyId, int secondCurrencyId)
    {
        var exchangeValue = await _dbContext.ExchangeValues.AsNoTracking()
                                                           .FirstOrDefaultAsync(exchangeValueItem => exchangeValueItem.FirstCurrencyId == firstCurrencyId &&
                                                                                                     exchangeValueItem.SecondCurrencyId == secondCurrencyId);
        return exchangeValue;
    }
    
    public async Task<decimal?> GetUSDExchangeValueByCurrencyTypeAsync(int? sourceCurrencyId, int? usdCurrencyId)
    {
        var exchangeValue = await _dbContext.ExchangeValues.AsNoTracking()
                                                           .FirstOrDefaultAsync(exchangeValueItem => exchangeValueItem.FirstCurrencyId == sourceCurrencyId && 
                                                                                                     exchangeValueItem.SecondCurrencyId == usdCurrencyId);

        return exchangeValue?.UnitOfFirstValue;
    }
     
    public async Task<ExchangeValue> AddExchangeValueAsync(ExchangeValue exchangeValue)
    {
        var exchangeValueReturned = await _dbContext.ExchangeValues.AddAsync(exchangeValue);

        return exchangeValueReturned.Entity;
    }
    
    public ExchangeValue UpdateExchangeValue(ExchangeValue exchangeValue, ExchangeValue updatedExchangeValue)
    {
        _dbContext.Entry(exchangeValue).Property(p => p.UnitOfFirstValue).IsModified = true;
        _dbContext.Entry(exchangeValue).Property(p => p.UnitOfSecondValue).IsModified = true;
        exchangeValue.UnitOfFirstValue = updatedExchangeValue.UnitOfFirstValue;
        exchangeValue.UnitOfSecondValue = updatedExchangeValue.UnitOfSecondValue;

        return exchangeValue;
    }
    
    public bool DeleteExchangeValue(ExchangeValue exchangeValue)
    {
        var entityEntry = _dbContext.ExchangeValues.Remove(exchangeValue);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}