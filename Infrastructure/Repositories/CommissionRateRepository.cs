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
        var commissionRatesList = await _dbContext.CommissionRates.AsNoTracking()
                                                                  .ToListAsync();
        return commissionRatesList;
    }

    public async Task<CommissionRate?> GetCommissionRateByMaxRangeAsync(decimal maxRange)
    {
        var commissionRate = await _dbContext.CommissionRates.AsNoTracking()
                                                             .FirstOrDefaultAsync(commissionRateItem => commissionRateItem.MaxUSDRange == maxRange);

        return commissionRate;
    }

    public async Task<decimal?> GetCRateByUSDAmountAsync(decimal amount)
    {
        var commissionRatesList = await _dbContext.CommissionRates.ToListAsync();
        if (commissionRatesList.Count == 0) return null;
        
        var cRateIndex = ~(commissionRatesList.Select(commissionRate => commissionRate.MaxUSDRange).Order().ToList().BinarySearch(amount));
        if (cRateIndex == commissionRatesList.Count) return null;
        
        var finalCRate = commissionRatesList.ElementAtOrDefault(cRateIndex)!.CRate;
        return finalCRate;
    }
    
    public async Task<CommissionRate> AddCommissionRateAsync(CommissionRate commissionRate)
    {
        var commissionRateReturned = await _dbContext.CommissionRates.AddAsync(commissionRate);

        return commissionRateReturned.Entity;
    }
    
    public CommissionRate UpdateCRate(CommissionRate commissionRate, decimal commissionCRate)
    {
        _dbContext.Entry(commissionRate).Property(p => p.CRate).IsModified = true;
        commissionRate.CRate = commissionCRate;

        return commissionRate;
    }
    
    public bool DeleteCommissionRate(CommissionRate commissionRate)
    {
        var entityEntry = _dbContext.CommissionRates.Remove(commissionRate);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}