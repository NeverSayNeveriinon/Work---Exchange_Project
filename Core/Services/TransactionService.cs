using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.TransactionDTO;
using Core.ServiceContracts;

namespace Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    
    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionResponse> AddTransaction(TransactionAddRequest? transactionAddRequest)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'TransactionRequest' object parameter is Null");
        
        // 'transactionAddRequest.Name' is valid and there is no problem //
        Transaction transaction = transactionAddRequest.ToTransaction();
        Transaction transactionReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        return transactionReturned.ToTransactionResponse();
    }   
    

    public async Task<List<TransactionResponse>> GetAllTransactions()
    {
        List<Transaction> transactions = await _transactionRepository.GetAllTransactionsAsync();
        
        List<TransactionResponse> transactionResponses = transactions.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }

    public async Task<TransactionResponse?> GetTransactionByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'Id' parameter is Null");

        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'transactions list' 
        if (transaction == null)
        {
            return null;
        }

        // if there is no problem
        TransactionResponse transactionResponse = transaction.ToTransactionResponse();

        return transactionResponse;
    }

    public async Task<TransactionResponse?> UpdateTransaction(TransactionUpdateRequest? transactionUpdateRequest, int? transactionID)
    {
        // if 'transaction ID' is null
        ArgumentNullException.ThrowIfNull(transactionID,"The Transaction'ID' parameter is Null");
        
        // if 'transactionUpdateRequest' is null
        ArgumentNullException.ThrowIfNull(transactionUpdateRequest,"The 'TransactionUpdateRequest' object parameter is Null");
        
        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(transactionID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null)
        {
            return null;
        }
            
        Transaction updatedTransaction = _transactionRepository.UpdateTransaction(transaction, transactionUpdateRequest.ToTransaction());
        await _transactionRepository.SaveChangesAsync();

        return updatedTransaction.ToTransactionResponse();
    }

    public async Task<bool?> DeleteTransaction(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'ID' parameter is Null");

        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null) 
        {
            return null;
        }
    
        bool result = _transactionRepository.DeleteTransaction(transaction);
        await _transactionRepository.SaveChangesAsync();

        return result;
    }
}