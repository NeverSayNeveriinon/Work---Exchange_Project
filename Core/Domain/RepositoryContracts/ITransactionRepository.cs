using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ITransactionRepository
{
    public Task<List<Transaction>> GetAllTransactionsAsync();
    public Task<Transaction?> GetTransactionByIDAsync(int id);
    public Task<Transaction> AddTransactionAsync(Transaction transaction);
    public Transaction Attach(Transaction transaction);
    public void LoadReferences(Transaction transaction);
    public Transaction UpdateTransaction(Transaction transaction, Transaction updatedTransaction);
    public bool DeleteTransaction(Transaction transaction);
    public Task SaveChangesAsync();
}