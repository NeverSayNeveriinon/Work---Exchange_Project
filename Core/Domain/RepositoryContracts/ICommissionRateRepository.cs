using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICommissionRateRepository
{
    public Task<List<CommissionRate>> GetAllCommissionRatesAsync();
    public Task<CommissionRate?> GetCommissionRateByIDAsync(int id);
    public Task<decimal> GetCRateByAmount(decimal amount);
    public Task<CommissionRate> AddCommissionRateAsync(CommissionRate account);
    public CommissionRate UpdateCommissionRate(CommissionRate account, CommissionRate updatedCommissionRate);
    public bool DeleteCommissionRate(CommissionRate account);
    public Task SaveChangesAsync();
    
}