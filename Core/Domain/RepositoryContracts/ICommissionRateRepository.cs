using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ICommissionRateRepository
{
    public Task<List<CommissionRate>> GetAllCommissionRates();
    public Task<CommissionRate?> GetCommissionRateByID(int id);
    public Task<CommissionRate> AddCommissionRate(CommissionRate account);
    public Task<CommissionRate> UpdateCommissionRate(CommissionRate account, CommissionRate updatedCommissionRate);
    public Task<bool> DeleteCommissionRate(CommissionRate account);
    public Task SaveChangesAsync();
}