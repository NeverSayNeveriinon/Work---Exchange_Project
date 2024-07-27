using Core.Domain.Entities;
using Core.Enums;

namespace Core.Domain.RepositoryContracts;

public interface ITransactionRepository
{
    public Task<List<Transaction>> GetAllTransactionsAsync(bool ignoreQueryFilter = false);
    public Task<Transaction?> GetTransactionByIDAsync(Guid id, bool ignoreQueryFilter = false);
    public Task<List<Transaction>> GetAllTransactionsByUserAsync(Guid ownerID);
    public Task<Transaction> AddTransactionAsync(Transaction transaction);
    public Transaction Attach(Transaction transaction);
    public void LoadReferences(Transaction transaction);
    // public Transaction UpdateTransaction(Transaction transaction, Transaction updatedTransaction);
    public Transaction UpdateTransactionStatusOfTransaction(Transaction transaction, TransactionStatusOptions transactionStatus);
    public void DeleteTransaction(Transaction transaction);
    public Task<int> SaveChangesAsync();
}