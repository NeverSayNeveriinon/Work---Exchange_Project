using Core.DTO.TransactionDTO;

namespace Core.ServiceContracts;

public interface ITransactionService
{
    public Task<TransactionResponse> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest);
    public Task<TransactionResponse> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest);
    public Task<List<TransactionResponse>> GetAllTransactions();
    public Task<TransactionResponse?> GetTransactionByID(int? ID);
    public Task<TransactionResponse?> UpdateTransaction(TransactionUpdateRequest? transactionUpdateRequest, int? transactionID);
    public Task<bool?> DeleteTransaction(int? ID);
}