using System.Security.Claims;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    public Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountAddRequest? CurrencyAccountAddRequest, ClaimsPrincipal userClaims);
    public Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts();
    public Task<CurrencyAccountResponse?> GetCurrencyAccountByID(int? ID);
    public Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountUpdateRequest? currencyAccountUpdateRequest, int? currencyAccountID);
    public Task<bool?> DeleteCurrencyAccount(int? ID);
    public Task<CurrencyAccountResponse?> IncreaseBalanceAmount(int? currencyAccountID, Money? money); 
}