using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    Task<(bool isValid, string? message, CurrencyAccountAddResponse? obj)> AddCurrencyAccount(CurrencyAccountAddRequest currencyAccountAddRequest, ClaimsPrincipal userClaims);
    Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts(ClaimsPrincipal userClaims);
    Task<List<CurrencyAccountResponse>> GetAllCurrencyAccountsInternal();
    
    Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims);
    Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumberInternal(string number);
    Task<(bool isValid, string? message, CurrencyAccount? obj)> GetCurrencyAccountByNumberWithNavigationInternal(string number);
    
    Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims);
    Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumberInternal(string number);
    
    CurrencyAccountResponse? UpdateBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal,decimal,decimal> calculationFunc);
    CurrencyAccountResponse? UpdateStashBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal, decimal, decimal> calculationFunc);
}