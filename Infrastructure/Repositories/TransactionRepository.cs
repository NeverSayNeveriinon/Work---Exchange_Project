using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _dbContext;

    
    public TransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
   

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        var transactions = _dbContext.Transactions.Include(property => property.FromAccount)
                                                  .ThenInclude(Property => Property.Currency)
                                                  .ThenInclude(Property => Property.FirstExchangeValues)
                                                  .Include(property => property.ToAccount)
                                                  .ThenInclude(property => property.Currency)
                                                  .AsNoTracking();
        
        List<Transaction> transactionsList = await transactions.ToListAsync();
        
        return transactionsList;
    }

    public async Task<Transaction?> GetTransactionByIDAsync(int id)
    {
        Transaction? transaction = await _dbContext.Transactions.Include(property => property.FromAccount)
                                                                .ThenInclude(Property => Property.Currency)
                                                                .ThenInclude(Property => Property.FirstExchangeValues)
                                                                .Include(property => property.ToAccount)
                                                                .ThenInclude(property => property.Currency)
                                                                .AsNoTracking()
                                                                .FirstOrDefaultAsync(transactionItem => transactionItem.Id == id);

        return transaction;
    }

     
    public async Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        var tranactionReturned = await _dbContext.Transactions.AddAsync(transaction);

        return tranactionReturned.Entity;
    }
    
    public Transaction Attach(Transaction transaction)
    {
        var transactionReturned = _dbContext.Transactions.Attach(transaction);

        return transactionReturned.Entity;
    }
    
    public void LoadReferences(Transaction transaction)
    {
        _dbContext.Entry(transaction).Reference(c => c.FromAccount).Load();
        _dbContext.Entry(transaction).Reference(c => c.ToAccount).Load();
    }
    
    public Transaction UpdateTransaction(Transaction transaction, Transaction updatedTransaction)
    {
        // _dbContext.Entry(transaction).Property(p => p.CurrencyID).IsModified = true;
        // transaction.CurrencyID = updatedTransaction.CurrencyID;
        
        return transaction;
    }
    
    public bool DeleteTransaction(Transaction transaction)
    {
        var entityEntry = _dbContext.Transactions.Remove(transaction);
        
        return entityEntry.State == EntityState.Deleted;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}