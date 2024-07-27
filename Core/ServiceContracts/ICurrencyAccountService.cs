using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    public Task<(bool isValid, string? message, CurrencyAccountAddResponse? obj)> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest, ClaimsPrincipal userClaims);
    public Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts(ClaimsPrincipal? userClaims);
    public Task<List<CurrencyAccountResponse>> GetAllCurrencyAccountsInternal();
    public Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumber(string? number, ClaimsPrincipal userClaims);
    public Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumberInternal(string? number);
    public Task<(bool isValid, string? message, CurrencyAccount? obj)> GetCurrencyAccountByNumberWithNavigationInternal(string? number);
    public Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumber(string? number, ClaimsPrincipal userClaims);
    public Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumberInternal(string? number);
    public Task<CurrencyAccountResponse?> UpdateBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal,decimal,decimal> calculationFunc, bool isTransactionConfirmed = false);
    public Task<CurrencyAccountResponse?> UpdateStashBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal, decimal, decimal> calculationFunc, bool isTransactionConfirmed = false);
}