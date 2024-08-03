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
using FluentResults;
using Microsoft.AspNetCore.Identity;
using static Core.Helpers.FluentResultsExtensions;

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

    public async Task<Result<CurrencyAccountAddResponse>> AddCurrencyAccount(CurrencyAccountAddRequest currencyAccountAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,$"The '{nameof(currencyAccountAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,$"The '{nameof(userClaims)}' object parameter is Null");
        
        var (currencyResult, currency) = (await _currencyService.GetCurrencyByCurrencyType(currencyAccountAddRequest.CurrencyType)).DeconstructObject();
        if (currencyResult.IsFailed) return currencyResult.ToResult(); // if 'currency' doesn't exist
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist

        var currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user.Id, currency.Id);
        
        var transactionAddRequest = new TransactionDepositAddRequest()
        {
            AccountNumber = currencyAccount.Number,
            Money = new MoneyRequest(){Amount = currencyAccountAddRequest.MoneyToOpenAccount.Amount!.Value, CurrencyType = currencyAccountAddRequest.MoneyToOpenAccount.CurrencyType}
        };
        
        var (transactionValidateResult, transactionResponse) = (await _transactionService.Value.AddOpenAccountDepositTransaction(transactionAddRequest, currencyAccountAddRequest)).DeconstructObject();
        // if (transactionValidateResult.IsFailed) return transactionValidateResult.ToResult();
        if (transactionValidateResult.IsFailed) return Result.Fail(transactionValidateResult.FirstErrorMessage());
        
        var currencyAccountReturned = await _accountRepository.AddCurrencyAccountAsync(currencyAccount);
        
        UpdateStashBalanceAmount(currencyAccount, transactionResponse.FromAccountChangeAmount, (val1, val2) => val1 + val2);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(currencyAccountReturned.ToCurrencyAccountResponse(transactionResponse.Id));
    }   
    
    public async Task<Result<List<CurrencyAccountResponse>>> GetAllCurrencyAccounts(ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist
        
        if (userClaims.IsInRole(Constants.AdminRole))
            return await GetAllCurrencyAccountsInternal();

        var currencyAccounts = await _accountRepository.GetAllCurrencyAccountsByUserAsync(user.Id);
        
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccountsInternal()
    {
        var currencyAccounts = await _accountRepository.GetAllCurrencyAccountsAsync();
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }
    
    public async Task<Result<CurrencyAccountResponse>> GetCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        if (userClaims.IsInRole(Constants.AdminRole))
            return await GetCurrencyAccountByNumberInternal(number);
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist
        
        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return Result.Fail(CreateNotFoundError("!!A Currency Account With This Number Has Not Been Found!!")); // if 'Number' is invalid (doesn't exist)
        
        if (currencyAccount.OwnerID != user.Id)
            return Result.Fail("This Account Number Doesn't Belong To You");
        
        return Result.Ok(currencyAccount.ToCurrencyAccountResponse());
    }
    
    public async Task<Result<CurrencyAccountResponse>> GetCurrencyAccountByNumberInternal(string number)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return Result.Fail(CreateNotFoundError("!!A Currency Account With This Number Has Not Been Found!!")); // if 'Number' is invalid (doesn't exist)

        return Result.Ok(currencyAccount.ToCurrencyAccountResponse());
    }   
    
    public async Task<Result<CurrencyAccount>> GetCurrencyAccountByNumberWithNavigationInternal(string number)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) Result.Fail(CreateNotFoundError("!!A Currency Account With This Number Has Not Been Found!!")); // if 'Number' is invalid (doesn't exist)

        return Result.Ok(currencyAccount!);
    }
    
    public async Task<Result> DeleteCurrencyAccountByNumber(string number, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(number,$"The CurrencyAccount'{nameof(number)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        if (userClaims.IsInRole(Constants.AdminRole))
            return await DeleteCurrencyAccountByNumberInternal(number);
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist
        
        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return Result.Fail(CreateNotFoundError("!!A Currency Account With This Number Has Not Been Found!!")); // if 'Number' is invalid (doesn't exist)
    
        if (currencyAccount.OwnerID != user.Id)
            return Result.Fail("This Account Number Doesn't Belong To You");
        
        _accountRepository.DeleteCurrencyAccount(currencyAccount);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Been Successful");
    }

    public async Task<Result> DeleteCurrencyAccountByNumberInternal(string number)
    {
        ArgumentNullException.ThrowIfNull(number,$"The '{nameof(number)}' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        if (currencyAccount == null) return Result.Fail(CreateNotFoundError("!!A Currency Account With This Number Has Not Been Found!!")); // if 'Number' is invalid (doesn't exist)
        
        _accountRepository.DeleteCurrencyAccount(currencyAccount);
        var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Been Successful");
    }
    
    
    public CurrencyAccountResponse UpdateStashBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal,decimal,decimal> calculationFunc)
    {
        ArgumentNullException.ThrowIfNull(currencyAccount, $"The '{nameof(currencyAccount)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(amount, $"The '{nameof(amount)}' parameter is Null");
        
        var updatedCurrencyAccount = _accountRepository.UpdateStashBalanceAmount(currencyAccount, amount, calculationFunc);
        
        // var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        // if (!(numberOfRowsAffected > 0)) return null;
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }    
    
    public CurrencyAccountResponse UpdateBalanceAmount(CurrencyAccount currencyAccount, decimal amount, Func<decimal,decimal,decimal> calculationFunc)
    {
        ArgumentNullException.ThrowIfNull(currencyAccount, $"The '{nameof(currencyAccount)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(amount, $"The '{nameof(amount)}' parameter is Null");

        var updatedCurrencyAccount = _accountRepository.UpdateBalanceAmount(currencyAccount, amount, calculationFunc);
        
        // var numberOfRowsAffected = await _accountRepository.SaveChangesAsync();
        // if (!(numberOfRowsAffected > 0)) return null;
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
}