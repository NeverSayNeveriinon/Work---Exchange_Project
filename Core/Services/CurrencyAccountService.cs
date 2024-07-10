using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Core.Services;

public class CurrencyAccountService : ICurrencyAccountService
{
    private readonly ICurrencyAccountRepository _accountRepository;
    private readonly UserManager<UserProfile> _userManager;

    public CurrencyAccountService(ICurrencyAccountRepository accountRepository, UserManager<UserProfile> userManager)
    {
        _accountRepository = accountRepository;
        _userManager = userManager;
    }

    public async Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest, ClaimsPrincipal userClaims)
    {
        // 'CurrencyAccountAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,"The 'CurrencyAccountAddRequest' object parameter is Null");
        
        // ValidationHelper.ModelValidation(currencyAccountAddRequest);

        // // 'CurrencyAccountAddRequest.Name' is Duplicate //
        // // Way 1
        // if ( (await _accountRepository.GetFilteredCurrencyAccounts(currencyAccount => currencyAccount.Name == CurrencyAccountAddRequest.Name))?.Count > 0)
        // {
        //     throw new ArgumentException("The 'CurrencyAccount Name' is already exists");
        // }
        
        
        // 'CurrencyAccountAddRequest.Name' is valid and there is no problem //
        var user = await _userManager.GetUserAsync(userClaims);
        CurrencyAccount currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user?.Id);
        CurrencyAccount currencyAccountReturned = await _accountRepository.AddCurrencyAccountAsync(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return currencyAccountReturned.ToCurrencyAccountResponse();
    }   
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts()
    {
        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        List<CurrencyAccount> currencyAccounts = await _accountRepository.GetAllCurrencyAccountsAsync();
        
        List<CurrencyAccountResponse> currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }

    public async Task<CurrencyAccountResponse?> GetCurrencyAccountByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CurrencyAccount'Id' parameter is Null");

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null)
        {
            return null;
        }

        // if there is no problem
        CurrencyAccountResponse currencyAccountResponse = currencyAccount.ToCurrencyAccountResponse();

        return currencyAccountResponse;;
    }

    public async Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountUpdateRequest? currencyAccountUpdateRequest, int? currencyAccountID)
    {
        // if 'currencyAccount ID' is null
        ArgumentNullException.ThrowIfNull(currencyAccountID,"The CurrencyAccount'ID' parameter is Null");
        
        // if 'currencyAccountUpdateRequest' is null
        ArgumentNullException.ThrowIfNull(currencyAccountUpdateRequest,"The 'CurrencyAccountUpdateRequest' object parameter is Null");

        
        // Validation
        // ValidationHelper.ModelValidation(CurrencyAccountUpdateRequest);

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByIDAsync(currencyAccountID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currencyAccount == null)
        {
            return null;
        }
            
        CurrencyAccount updatedCurrencyAccount = _accountRepository.UpdateCurrencyAccount(currencyAccount, currencyAccountUpdateRequest.ToCurrencyAccount());
        await _accountRepository.SaveChangesAsync();

        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }

    public async Task<bool?> DeleteCurrencyAccount(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CurrencyAccount'ID' parameter is Null");

        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currencyAccount == null) 
        {
            return null;
        }
    
        bool result = _accountRepository.DeleteCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return result;
    }
    
    

    public async Task<CurrencyAccountResponse?> IncreaseBalanceAmount(int? currencyAccountID, Money? money)
    {
        // if 'currencyAccountID' is null
        ArgumentNullException.ThrowIfNull(currencyAccountID,"The 'currencyAccountID' parameter is Null");
        
        // if 'money' is null
        ArgumentNullException.ThrowIfNull(money,"The 'money' object parameter is Null");

        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByIDAsync(currencyAccountID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currencyAccount == null)
        {
            return null;
        }
            
        CurrencyAccount updatedCurrencyAccount = _accountRepository.UpdateBalanceAmount(currencyAccount, money.Amount);
        await _accountRepository.SaveChangesAsync();

        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
}