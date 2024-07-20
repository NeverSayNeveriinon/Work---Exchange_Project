using System.Security.Claims;
using Core.DTO.TransactionDTO;

namespace Core.ServiceContracts;

public interface ITransactionService
{
    public Task<(bool isValid, string? message, TransactionResponse? obj)> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest, ClaimsPrincipal userClaims);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest, ClaimsPrincipal userClaims);
    public Task<List<TransactionResponse>> GetAllTransactions(ClaimsPrincipal userClaims);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByID(int? Id, ClaimsPrincipal userClaims);
    // public Task<TransactionResponse?> UpdateTransaction(TransactionUpdateRequest? transactionUpdateRequest, int? transactionID);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> UpdateIsConfirmedOfTransaction(int? transactionId, ClaimsPrincipal userClaims, bool? isConfirmed, TimeSpan TimeNow);
    public Task<(bool isValid, bool isFound, string? message)> DeleteTransaction(int? Id, ClaimsPrincipal userClaims);
}