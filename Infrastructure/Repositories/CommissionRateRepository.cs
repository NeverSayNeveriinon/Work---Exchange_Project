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
    
    public void DeleteCommissionRate(CommissionRate commissionRate)
    {
        _dbContext.CommissionRates.Remove(commissionRate);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}