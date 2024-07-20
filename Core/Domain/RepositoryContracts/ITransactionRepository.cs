using Core.Domain.Entities;

namespace Core.Domain.RepositoryContracts;

public interface ITransactionRepository
{
    public Task<List<Transaction>> GetAllTransactionsAsync();
    public Task<Transaction?> GetTransactionByIDAsync(int id);
    public Task<List<Transaction>> GetAllTransactionsByUserAsync(Guid ownerID);
    public Task<Transaction> AddTransactionAsync(Transaction transaction);
    public Transaction Attach(Transaction transaction);
    public void LoadReferences(Transaction transaction);
    // public Transaction UpdateTransaction(Transaction transaction, Transaction updatedTransaction);
    public Transaction UpdateIsConfirmedOfTransaction(Transaction transaction, bool isConfirmed);
    public bool DeleteTransaction(Transaction transaction);
    public Task SaveChangesAsync();
}