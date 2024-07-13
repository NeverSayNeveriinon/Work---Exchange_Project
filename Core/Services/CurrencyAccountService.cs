using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.Enums;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Core.Services;

public class CurrencyAccountService : ICurrencyAccountService
{
    private readonly ICurrencyAccountRepository _accountRepository;
    private readonly ICurrencyService _currencyService;
    private readonly UserManager<UserProfile> _userManager;

    public CurrencyAccountService(ICurrencyAccountRepository accountRepository, UserManager<UserProfile> userManager, ICurrencyService currencyService)
    {
        _accountRepository = accountRepository;
        _userManager = userManager;
        _currencyService = currencyService;
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
        var currency = await _currencyService.GetCurrencyByCurrencyType(currencyAccountAddRequest.CurrencyType);
        var user = await _userManager.GetUserAsync(userClaims);
        CurrencyAccount currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user?.Id, currency?.Id);
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

    public async Task<CurrencyAccountResponse?> GetCurrencyAccountByNumber(int? number)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'number' parameter is Null");

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number.Value);

        // if 'id' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null)
        {
            return null;
        }

        // if there is no problem
        CurrencyAccountResponse currencyAccountResponse = currencyAccount.ToCurrencyAccountResponse();

        return currencyAccountResponse;;
    }

    public async Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountUpdateRequest? currencyAccountUpdateRequest, int? currencyAccountNumber)
    {
        // if 'currencyAccount Number' is null
        ArgumentNullException.ThrowIfNull(currencyAccountNumber,"The CurrencyAccount'Number' parameter is Null");
        
        // if 'currencyAccountUpdateRequest' is null
        ArgumentNullException.ThrowIfNull(currencyAccountUpdateRequest,"The 'CurrencyAccountUpdateRequest' object parameter is Null");

        
        // Validation
        // ValidationHelper.ModelValidation(CurrencyAccountUpdateRequest);

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(currencyAccountNumber.Value);
        
        // if 'Number' is invalid (doesn't exist)
        if (currencyAccount == null)
        {
            return null;
        }
            
        var currency = await _currencyService.GetCurrencyByCurrencyType(currencyAccountUpdateRequest.CurrencyType);
        CurrencyAccount updatedCurrencyAccount = _accountRepository.UpdateCurrencyAccount(currencyAccount, currencyAccountUpdateRequest.ToCurrencyAccount(currency.Id));
        await _accountRepository.SaveChangesAsync();

        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
    public async Task<bool?> DeleteCurrencyAccount(int? number)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'Number' parameter is Null");

        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number.Value);
        
        // if 'Number' is invalid (doesn't exist)
        if (currencyAccount == null) 
        {
            return null;
        }
    
        bool result = _accountRepository.DeleteCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return result;
    }
    
    

    public async Task<CurrencyAccountResponse?> IncreaseBalanceAmount(int? currencyAccountNumber, Money? money)
    {
        // if 'currencyAccountNumber' is null
        ArgumentNullException.ThrowIfNull(currencyAccountNumber,"The 'currencyAccountNumber' parameter is Null");
        
        // if 'money' is null
        ArgumentNullException.ThrowIfNull(money,"The 'money' object parameter is Null");

        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(currencyAccountNumber.Value);
        
        // if 'Number' is invalid (doesn't exist)
        if (currencyAccount == null)
        {
            return null;
        }
        
        var moneyCurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), money.CurrencyType);
        var valueToBeMultiplied = currencyAccount?.Currency?.SecondExchangeValues?.FirstOrDefault(exValue=> exValue.FirstCurrency.CurrencyType == moneyCurrencyType).UnitOfFirstValue;
        var amount = money.Amount * valueToBeMultiplied.Value;
        
        
        CurrencyAccount updatedCurrencyAccount = _accountRepository.UpdateBalanceAmount(currencyAccount, amount);
        await _accountRepository.SaveChangesAsync();

        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
}