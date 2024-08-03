using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using FluentResults;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    Task<Result<CurrencyAccountAddResponse>> AddCurrencyAccount(CurrencyAccountAddRequest currencyAccountAddRequest, ClaimsPrincipal userClaims);
    Task<Result<List<CurrencyAccountResponse>>> GetAllCurrencyAccounts(ClaimsPrincipal userClaims);
    Task<List<CurrencyAccountResponse>> GetAllCurrencyAccountsInternal();
    
    Task<Result<CurrencyAccountResponse>> GetCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims);
    Task<Result<CurrencyAccountResponse>> GetCurrencyAccountByNumberInternal(string number);
    Task<Result<CurrencyAccount>> GetCurrencyAccountByNumberWithNavigationInternal(string number);
    
    Task<Result> DeleteCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims);
    Task<Result> DeleteCurrencyAccountByNumberInternal(string number);
    
    CurrencyAccountResponse UpdateBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal,decimal,decimal> calculationFunc);
    CurrencyAccountResponse UpdateStashBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal, decimal, decimal> calculationFunc);
}