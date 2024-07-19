using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CommissionRateRepository : ICommissionRateRepository
{
    private readonly AppDbContext _dbContext;

    
    public CommissionRateRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
   

    public async Task<List<CommissionRate>> GetAllCommissionRatesAsync()
    {
        var commissionRates = _dbContext.CommissionRates.AsNoTracking();
        
        List<CommissionRate> commissionRatesList = await commissionRates.ToListAsync();
        
        return commissionRatesList;
    }

    public async Task<CommissionRate?> GetCommissionRateByIDAsync(int id)
    {
        CommissionRate? currency = await _dbContext.CommissionRates
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(currencyItem => currencyItem.Id == id);

        return currency;
    }

    public async Task<decimal> GetCRateByAmountAsync(decimal amount)
    {
        var f = await _dbContext.CommissionRates.ToListAsync();
        var g = ~(f.Select(commissionRate => commissionRate.CRate).ToList().BinarySearch(amount));
        var j = f.ElementAtOrDefault(g).CRate;
        return j;
    }
    
    public async Task<CommissionRate> AddCommissionRateAsync(CommissionRate currency)
    {
        var currencyReturned = await _dbContext.CommissionRates.AddAsync(currency);

        return currencyReturned.Entity;
    }
    
    public CommissionRate UpdateCommissionRate(CommissionRate currency, CommissionRate updatedCommissionRate)
    {
        _dbContext.Entry(currency).Property(p => p.CRate).IsModified = true;
        _dbContext.Entry(currency).Property(p => p.MaxUSDRange).IsModified = true;
        
        currency.CRate = updatedCommissionRate.CRate;
        currency.MaxUSDRange = updatedCommissionRate.MaxUSDRange;

        return currency;
    }
    
    public bool DeleteCommissionRate(CommissionRate currency)
    {
        var entityEntry = _dbContext.CommissionRates.Remove(currency);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}