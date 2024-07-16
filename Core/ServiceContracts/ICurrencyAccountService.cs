using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    public Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountAddRequest? CurrencyAccountAddRequest, ClaimsPrincipal userClaims);
    public Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts();
    public Task<CurrencyAccountResponse?> GetCurrencyAccountByNumber(int? Number);
    public Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountUpdateRequest? currencyAccountUpdateRequest, int? currencyAccountNumber);
    public Task<bool?> DeleteCurrencyAccount(int? Number);
    public Task<CurrencyAccountResponse?> UpdateBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal, decimal, decimal> calculationFunc);
}