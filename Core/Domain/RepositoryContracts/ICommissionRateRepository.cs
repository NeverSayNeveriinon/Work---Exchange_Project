using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICommissionRateRepository
{
    public Task<List<CommissionRate>> GetAllCommissionRatesAsync();
    public Task<CommissionRate?> GetCommissionRateByMaxRangeAsync(decimal maxRange);
    public Task<decimal?> GetCRateByUSDAmountAsync(decimal amount);
    public Task<CommissionRate> AddCommissionRateAsync(CommissionRate commissionRate);
    public CommissionRate UpdateCRate(CommissionRate commissionRate, decimal commissionCRate);
    public void DeleteCommissionRate(CommissionRate commissionRate);
    public Task<int> SaveChangesAsync();
    
}