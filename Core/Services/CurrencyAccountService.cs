using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Helpers;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Core.Services;

// todo: check response of 'SaveChangesAsync'
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

    public async Task<(bool isValid, string? message, CurrencyAccountAddResponse? obj)> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,"The 'CurrencyAccountAddRequest' object parameter is Null");
        
        var currency = await _currencyService.GetCurrencyByCurrencyType(currencyAccountAddRequest.CurrencyType);
        var user = await _userManager.GetUserAsync(userClaims);
        
        var currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user!.Id, currency!.Id);
        
        var transactionAddRequest = new TransactionDepositAddRequest()
        {
            AccountNumber = currencyAccount.Number,
            Money = new MoneyRequest(){Amount = currencyAccountAddRequest.MoneyToOpenAccount.Amount!.Value, CurrencyType = currencyAccountAddRequest.MoneyToOpenAccount.CurrencyType}
        };
        
        var (isValid, message, tranasctionResponse) = await _transactionService.Value.AddOpenAccountDepositTransaction(transactionAddRequest, currencyAccountAddRequest);
        if (!isValid)
            return (false, message, null);
        
        var currencyAccountReturned = await _accountRepository.AddCurrencyAccountAsync(currencyAccount);
        
        await UpdateStashBalanceAmount(currencyAccount, tranasctionResponse!.FromAccountChangeAmount, (val1, val2) => val1 + val2);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        var currencyAccountResponse = currencyAccountReturned.ToCurrencyAccountResponse(tranasctionResponse.Id);
        return (true, null, currencyAccountResponse);
    }   
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts(ClaimsPrincipal? userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);
        
        List<CurrencyAccount> currencyAccounts;
        if (userClaims.IsInRole(Constants.AdminRole))
            return await GetAllCurrencyAccountsInternal();

        currencyAccounts = await _accountRepository.GetAllCurrencyAccountsByUserAsync(user!.Id);
        
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccountsInternal()
    {
        var currencyAccounts = await _accountRepository.GetAllCurrencyAccountsAsync();
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }
    
    public async Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumber(string? number, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'number' parameter is Null");
        
        if (userClaims.IsInRole(Constants.AdminRole))
            return await GetCurrencyAccountByNumberInternal(number);
        
        var user = await _userManager.GetUserAsync(userClaims);
        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);

        // if 'number' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null) 
            return (false, null, null);
        
        if (currencyAccount.OwnerID != user!.Id)
            return (false, "This Account Number Doesn't Belong To You", null);
        
        var currencyAccountResponse = currencyAccount.ToCurrencyAccountResponse();
        return (true, null, currencyAccountResponse);
    }
    
    public async Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumberInternal(string? number)
    {
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'number' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        
        // if 'number' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null) 
            return (false, null, null);

        var currencyAccountResponse = currencyAccount.ToCurrencyAccountResponse();
        return (true, null, currencyAccountResponse);
    }   
    
    public async Task<(bool isValid, string? message, CurrencyAccount? obj)> GetCurrencyAccountByNumberWithNavigationInternal(string? number)
    {
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'number' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        
        // if 'number' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null) 
            return (false, null, null);

        return (true, null, currencyAccount);
    }
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumber(string? number, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'Number' parameter is Null");
        
        if (userClaims.IsInRole(Constants.AdminRole))
            return await DeleteCurrencyAccountByNumberInternal(number);
        
        var user = await _userManager.GetUserAsync(userClaims);
        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        
        // if 'Number' is invalid (doesn't exist)
        if (currencyAccount == null)
            return (false, false, null);
    
        if (currencyAccount.OwnerID != user!.Id)
            return (false, true, "This Account Number Doesn't Belong To You");
        
        _accountRepository.DeleteCurrencyAccount(currencyAccount);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, "The Deletion Has Not Been Successful");
    }

    public async Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccountByNumberInternal(string? number)
    {
        ArgumentNullException.ThrowIfNull(number,"The 'number' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        
        // if 'number' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null) 
            return (false, false, null);
        
        _accountRepository.DeleteCurrencyAccount(currencyAccount);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, null);
    }
    
    public async Task<CurrencyAccountResponse?> UpdateStashBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal,decimal,decimal> calculationFunc, bool isTransactionConfirmed = false)
    {
        ArgumentNullException.ThrowIfNull(currencyAccount, "The 'currencyAccount' parameter is Null");
        ArgumentNullException.ThrowIfNull(amount, "The 'amount' parameter is Null");
        
        var updatedCurrencyAccount = _accountRepository.UpdateStashBalanceAmount(currencyAccount, amount.Value, calculationFunc);
        
        // var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        // if (!(numberOfRowsAffected > 0)) return null;
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }    
    
    public async Task<CurrencyAccountResponse?> UpdateBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal,decimal,decimal> calculationFunc, bool isTransactionConfirmed = false)
    {
        ArgumentNullException.ThrowIfNull(currencyAccount, "The 'currencyAccount' parameter is Null");
        ArgumentNullException.ThrowIfNull(amount, "The 'amount' parameter is Null");

        var updatedCurrencyAccount =_accountRepository.UpdateBalanceAmount(currencyAccount, amount.Value, calculationFunc);
        
        // var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        // if (!(numberOfRowsAffected > 0)) return null;
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
}