using Core.Domain.Entities;
using Core.DTO.CurrencyAccountDTO;

namespace Core.ServiceContracts;

public interface ICurrencyAccountService
{
    public Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountRequest? movieAddRequest);
    public Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts();
    public Task<CurrencyAccountResponse?> GetCurrencyAccountByID(int? ID);
    public Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountRequest? movieUpdateRequest, int? movieID);
    public Task<bool?> DeleteCurrencyAccount(int? ID);
}