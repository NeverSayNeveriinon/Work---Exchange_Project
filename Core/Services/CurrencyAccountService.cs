using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.ServicesDTO.Money;
using Core.DTO.TransactionDTO;
using Core.Helpers;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Core.Services;

public class CurrencyAccountService : ICurrencyAccountService
{
    private readonly ICurrencyAccountRepository _accountRepository;
    private readonly Lazy<ITransactionService> _transactionService;
    private readonly ICurrencyService _currencyService;
    private readonly UserManager<UserProfile> _userManager;

    public CurrencyAccountService(ICurrencyAccountRepository accountRepository, UserManager<UserProfile> userManager, ICurrencyService currencyService, Lazy<ITransactionService> transactionService)
    {
        _accountRepository = accountRepository;
        _userManager = userManager;
        _currencyService = currencyService;
        _transactionService = transactionService;
    }

    public async Task<(bool isValid, string? message, CurrencyAccountAddResponse? obj)> AddCurrencyAccount(CurrencyAccountAddRequest currencyAccountAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,$"The '{nameof(currencyAccountAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,$"The '{nameof(userClaims)}' object parameter is Null");
        
        var currency = await _currencyService.GetCurrencyByCurrencyType(currencyAccountAddRequest.CurrencyType);
        if (currency == null) return (false, "The Currency Type Doesn't Exist", null); // if 'currency' doesn't exist
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist

        var currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user.Id, currency!.Id);
        
        var transactionAddRequest = new TransactionDepositAddRequest()
        {
            AccountNumber = currencyAccount.Number,
            Money = new MoneyRequest(){Amount = currencyAccountAddRequest.MoneyToOpenAccount.Amount!.Value, CurrencyType = currencyAccountAddRequest.MoneyToOpenAccount.CurrencyType}
        };
        
        var (isValid, message, tranasctionResponse) = await _transactionService.Value.AddOpenAccountDepositTransaction(transactionAddRequest, currencyAccountAddRequest);
        if (!isValid)
            return (false, message, null);
        
        var currencyAccountReturned = await _accountRepository.AddCurrencyAccountAsync(currencyAccount);
        
        UpdateStashBalanceAmount(currencyAccount, tranasctionResponse!.FromAccountChangeAmount, (val1, val2) => val1 + val2);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        return (true, null, currencyAccountReturned.ToCurrencyAccountResponse(tranasctionResponse.Id));
    }   
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts(ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        // if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist
        
        if (userClaims.IsInRole(Constants.AdminRole))
            return await GetAllCurrencyAccountsInternal();

        var currencyAccounts = await _accountRepository.GetAllCurrencyAccountsByUserAsync(user!.Id);
        
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccountsInternal()
    {
        var currencyAccounts = await _accountRepository.GetAllCurrencyAccountsAsync();
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }
    
    public async Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        if (userClaims.IsInRole(Constants.AdminRole))
            return await GetCurrencyAccountByNumberInternal(number);
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist
        
        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return (false, null, null); // if 'Number' is invalid (doesn't exist)
        
        if (currencyAccount.OwnerID != user!.Id)
            return (false, "This Account Number Doesn't Belong To You", null);
        
        return (true, null, currencyAccount.ToCurrencyAccountResponse());
    }
    
    public async Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumberInternal(string number)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return (false, null, null); // if 'Number' is invalid (doesn't exist)

        return (true, null, currencyAccount.ToCurrencyAccountResponse());
    }   
    
    public async Task<(bool isValid, string? message, CurrencyAccount? obj)> GetCurrencyAccountByNumberWithNavigationInternal(string number)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return (false, null, null); // if 'Number' is invalid (doesn't exist)

        return (true, null, currencyAccount);
    }
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        if (userClaims.IsInRole(Constants.AdminRole))
            return await DeleteCurrencyAccountByNumberInternal(number);
        
        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return (false, false, null); // if 'Number' is invalid (doesn't exist)
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, true, "The User Doesn't Exist"); // if 'user' doesn't exist
    
        if (currencyAccount.OwnerID != user!.Id)
            return (false, true, "This Account Number Doesn't Belong To You");
        
        _accountRepository.DeleteCurrencyAccount(currencyAccount);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, "The Deletion Has Been Successful");
    }

    public async Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumberInternal(string number)
    {
        ArgumentNullException.ThrowIfNull(number,$"The '{nameof(number)}' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return (false, false, null); // if 'number' doesn't exist in 'currencyAccounts list' 
        
        _accountRepository.DeleteCurrencyAccount(currencyAccount);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, null);
    }
    
    public CurrencyAccountResponse? UpdateStashBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal,decimal,decimal> calculationFunc)
    {
        ArgumentNullException.ThrowIfNull(currencyAccount, $"The '{nameof(currencyAccount)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(amount, $"The '{nameof(amount)}' parameter is Null");
        
        var updatedCurrencyAccount = _accountRepository.UpdateStashBalanceAmount(currencyAccount, amount, calculationFunc);
        
        // var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        // if (!(numberOfRowsAffected > 0)) return null;
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }    
    
    public CurrencyAccountResponse? UpdateBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal,decimal,decimal> calculationFunc)
    {
        ArgumentNullException.ThrowIfNull(currencyAccount, $"The '{nameof(currencyAccount)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(amount, $"The '{nameof(amount)}' parameter is Null");

        var updatedCurrencyAccount = _accountRepository.UpdateBalanceAmount(currencyAccount, amount, calculationFunc);
        
        // var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        // if (!(numberOfRowsAffected > 0)) return null;
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
}