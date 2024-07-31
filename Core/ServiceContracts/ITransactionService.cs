using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Enums;
using FluentResults;

namespace Core.ServiceContracts;

public interface ITransactionService
{
    Task<Result<TransactionResponse>> AddTransferTransaction(TransactionTransferAddRequest transactionAddRequest, ClaimsPrincipal userClaims);
    Task<Result<TransactionResponse>> AddDepositTransaction(TransactionDepositAddRequest transactionAddRequest, ClaimsPrincipal userClaims);
    Task<Result<TransactionResponse>> AddOpenAccountDepositTransaction(TransactionDepositAddRequest transactionAddRequest, CurrencyAccountAddRequest currencyAccountAddRequest);
    
    Task<Result<List<TransactionResponse>>> GetAllTransactions(ClaimsPrincipal userClaims);
    Task<List<TransactionResponse>> GetAllTransactionsInternal();
    
    Task<Result<TransactionResponse>> GetTransactionByID(Guid id, ClaimsPrincipal userClaims, bool ignoreQueryFilter = false);
    Task<Result<TransactionResponse>> GetTransactionByIDInternal(Guid number);
    
    Task<Result> DeleteTransactionById(Guid id, ClaimsPrincipal userClaims);
    Task<Result> DeleteTransactionByIdInternal(Guid id);
    
    Task<Result<TransactionResponse>> UpdateTransactionStatusOfTransaction(ConfirmTransactionRequest confirmTransactionRequest, ClaimsPrincipal userClaims, DateTime DateTimeNow);
}