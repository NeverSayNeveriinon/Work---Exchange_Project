using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Enums;

namespace Core.ServiceContracts;

public interface ITransactionService
{
    public Task<(bool isValid, string? message, TransactionResponse? obj)> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest, ClaimsPrincipal userClaims);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest, ClaimsPrincipal userClaims);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> AddOpenAccountDepositTransaction(TransactionDepositAddRequest? transactionAddRequest, CurrencyAccountAddRequest currencyAccountAddRequest);
    public Task<List<TransactionResponse>> GetAllTransactions(ClaimsPrincipal userClaims);
    public Task<List<TransactionResponse>> GetAllTransactionsInternal();
    public Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByID(Guid? Id, ClaimsPrincipal userClaims, bool ignoreQueryFilter = false);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByIDInternal(Guid? number);
    public Task<(bool isValid, string? message, TransactionResponse? obj)> UpdateTransactionStatusOfTransaction(ConfirmTransactionRequest? confirmTransactionRequest, ClaimsPrincipal userClaims, DateTime DateTimeNow);
    public Task<(bool isValid, bool isFound, string? message)> DeleteTransactionById(Guid? Id, ClaimsPrincipal userClaims);
    public Task<(bool isValid, bool isFound, string? message)> DeleteTransactionByIdInternal(Guid? Id);
    public Task<(bool isValid, string? message)> CheckMinimumUSDBalanceAsync(string currencyType, decimal finalAmount);
}