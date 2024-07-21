using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    public Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest, ClaimsPrincipal userClaims);
    public Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts(ClaimsPrincipal? userClaims);
    public List<CurrencyAccountResponse> GetAllCurrencyAccountsInternal();
    public Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumber(string? number, ClaimsPrincipal userClaims);
    public (bool isValid, string? message, CurrencyAccountResponse? obj) GetCurrencyAccountByNumberInternal(string? number);
    public Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccount(string? number, ClaimsPrincipal userClaims);
    public Task<CurrencyAccountResponse?> UpdateBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal, decimal, decimal> calculationFunc);
}