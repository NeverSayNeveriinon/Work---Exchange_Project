using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.Enums;
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
   

    public async Task<List<Transaction>> GetAllTransactionsAsync(bool ignoreQueryFilter = false)
    {
        IQueryable<Transaction> transactionsQueryable = _dbContext.Transactions.AsQueryable();
        if (ignoreQueryFilter)
            transactionsQueryable = transactionsQueryable.IgnoreQueryFilters();
        
        var transactions = transactionsQueryable.Include(property => property.FromAccount)
                                                .ThenInclude(property => property!.Currency)
                                                .ThenInclude(property => property!.FirstExchangeValues)
                                                .Include(property => property.ToAccount)
                                                .ThenInclude(property => property!.Currency)
                                                .ThenInclude(property => property!.FirstExchangeValues)
                                                .AsNoTracking();
      
        var transactionsList = await transactions.ToListAsync();
        return transactionsList;
    }
    
    public async Task<List<Transaction>> GetAllTransactionsByUserAsync(Guid ownerID)
    {
        var transactions = _dbContext.Transactions.Include(property => property.FromAccount)
                                                  .ThenInclude(property => property!.Currency)
                                                  .ThenInclude(property => property!.FirstExchangeValues)
                                                  .Include(property => property.ToAccount)
                                                  .ThenInclude(property => property!.Currency)
                                                  .AsNoTracking()
                                                  .Where(transactionItem => transactionItem.FromAccount!.OwnerID == ownerID ||
                                                                            transactionItem.ToAccount!.OwnerID == ownerID);
        
        var transactionsList = await transactions.ToListAsync();
        return transactionsList;
    }

    public async Task<Transaction?> GetTransactionByIDAsync(Guid id, bool ignoreQueryFilter = false)
    {
        IQueryable<Transaction> transactionsQueryable = _dbContext.Transactions.AsQueryable();
        if (ignoreQueryFilter)
            transactionsQueryable = transactionsQueryable.IgnoreQueryFilters();
        
        var transaction = await transactionsQueryable.Include(property => property.FromAccount)
                                                     .ThenInclude(property => property!.Currency)
                                                     .ThenInclude(property => property!.FirstExchangeValues)
                                                     .Include(property => property.ToAccount)
                                                     .ThenInclude(property => property!.Currency)
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
    
    public void LoadAccountReferences(Transaction transaction)
    {
        _dbContext.Entry(transaction).Reference<CurrencyAccount>(c => c.FromAccount).Load();
        _dbContext.Entry(transaction).Reference<CurrencyAccount>(c => c.ToAccount).Load();
    }
    
    public Transaction UpdateTransactionStatusOfTransaction(Transaction transaction, TransactionStatusOptions transactionStatus)
    {
        _dbContext.Entry(transaction).Property(p => p.TransactionStatus).IsModified = true;
        transaction.TransactionStatus = transactionStatus;
        
        return transaction;
    }
    
    
    public void DeleteTransaction(Transaction transaction)
    {
        _dbContext.Transactions.Remove(transaction);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}