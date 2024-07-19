using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Enums;
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

    public async Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest, ClaimsPrincipal userClaims)
    {
        // 'CurrencyAccountAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,"The 'CurrencyAccountAddRequest' object parameter is Null");
        
        var currency = await _currencyService.GetCurrencyByCurrencyType(currencyAccountAddRequest.CurrencyType);
        var user = await _userManager.GetUserAsync(userClaims);
        
        CurrencyAccount currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user?.Id, currency?.Id);
        
        var transactionAddRequest = new TransactionDepositAddRequest()
        {
            AccountNumber = currencyAccount.Number,
            Money = new MoneyRequest(){Amount = currencyAccountAddRequest.MoneyToOpenAccount.Amount.Value, CurrencyType = currencyAccountAddRequest.MoneyToOpenAccount.CurrencyType}
        };
                
        var f = await _transactionService.Value.AddDepositTransaction(transactionAddRequest);

        
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

    public async Task<CurrencyAccountResponse?> GetCurrencyAccountByNumber(string? number)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'number' parameter is Null");

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);

        // if 'id' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null)
        {
            return null;
        }

        // if there is no problem
        CurrencyAccountResponse currencyAccountResponse = currencyAccount.ToCurrencyAccountResponse();

        return currencyAccountResponse;;
    }

    public async Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountUpdateRequest? currencyAccountUpdateRequest, string? currencyAccountNumber)
    {
        // if 'currencyAccount Number' is null
        ArgumentNullException.ThrowIfNull(currencyAccountNumber,"The CurrencyAccount'Number' parameter is Null");
        
        // if 'currencyAccountUpdateRequest' is null
        ArgumentNullException.ThrowIfNull(currencyAccountUpdateRequest,"The 'CurrencyAccountUpdateRequest' object parameter is Null");

        
        // Validation
        // ValidationHelper.ModelValidation(CurrencyAccountUpdateRequest);

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(currencyAccountNumber);
        
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
    
    public async Task<bool?> DeleteCurrencyAccount(string? number)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'Number' parameter is Null");

        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        
        // if 'Number' is invalid (doesn't exist)
        if (currencyAccount == null) 
        {
            return null;
        }
    
        bool result = _accountRepository.DeleteCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return result;
    }
    
    
    public async Task<CurrencyAccountResponse?> UpdateBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal,decimal,decimal> calculationFunc)
    {
        // if 'currencyAccount' is null
        ArgumentNullException.ThrowIfNull(currencyAccount, "The 'currencyAccount' parameter is Null");
        
        // if 'amount' is null
        ArgumentNullException.ThrowIfNull(amount, "The 'amount' parameter is Null");
        
        CurrencyAccount updatedCurrencyAccount = _accountRepository.UpdateBalanceAmount(currencyAccount, amount.Value, calculationFunc);
        await _accountRepository.SaveChangesAsync();
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
}