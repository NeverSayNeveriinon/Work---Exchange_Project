using Core.Domain.Entities;
using Core.Enums;

namespace Core.Domain.RepositoryContracts;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetAllTransactionsAsync(bool ignoreQueryFilter = false);
    Task<List<Transaction>> GetAllTransactionsByUserAsync(Guid ownerID);
    
    Task<Transaction?> GetTransactionByIDAsync(Guid id, bool ignoreQueryFilter = false);
    Task<Transaction> AddTransactionAsync(Transaction transaction);
    Transaction UpdateTransactionStatusOfTransaction(Transaction transaction, TransactionStatusOptions transactionStatus);
    void DeleteTransaction(Transaction transaction);
    Task<int> SaveChangesAsync();
    
    Transaction Attach(Transaction transaction);
    void LoadAccountReferences(Transaction transaction);
    // public Transaction UpdateTransaction(Transaction transaction, Transaction updatedTransaction);
}