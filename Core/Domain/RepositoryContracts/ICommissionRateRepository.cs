using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICommissionRateRepository
{
    Task<List<CommissionRate>> GetAllCommissionRatesAsync();
    
    Task<CommissionRate?> GetCommissionRateByMaxRangeAsync(decimal maxRange);
    
    Task<CommissionRate> AddCommissionRateAsync(CommissionRate commissionRate);
    CommissionRate UpdateCRate(CommissionRate commissionRate, decimal commissionCRate);
    void DeleteCommissionRate(CommissionRate commissionRate);
    Task<int> SaveChangesAsync();
    
}