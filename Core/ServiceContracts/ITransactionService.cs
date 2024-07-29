using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Enums;

namespace Core.ServiceContracts;

public interface ITransactionService
{
    Task<(bool isValid, string? message, TransactionResponse? obj)> AddTransferTransaction(TransactionTransferAddRequest transactionAddRequest, ClaimsPrincipal userClaims);
    Task<(bool isValid, string? message, TransactionResponse? obj)> AddDepositTransaction(TransactionDepositAddRequest transactionAddRequest, ClaimsPrincipal userClaims);
    Task<(bool isValid, string? message, TransactionResponse? obj)> AddOpenAccountDepositTransaction(TransactionDepositAddRequest transactionAddRequest, CurrencyAccountAddRequest currencyAccountAddRequest);
    
    Task<List<TransactionResponse>> GetAllTransactions(ClaimsPrincipal userClaims);
    Task<List<TransactionResponse>> GetAllTransactionsInternal();
    
    Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByID(Guid id, ClaimsPrincipal userClaims, bool ignoreQueryFilter = false);
    Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByIDInternal(Guid number);
    
    Task<(bool isValid, bool isFound, string? message)> DeleteTransactionById(Guid id, ClaimsPrincipal userClaims);
    Task<(bool isValid, bool isFound, string? message)> DeleteTransactionByIdInternal(Guid id);
    
    Task<(bool isValid, string? message, TransactionResponse? obj)> UpdateTransactionStatusOfTransaction(ConfirmTransactionRequest confirmTransactionRequest, ClaimsPrincipal userClaims, DateTime DateTimeNow);
}