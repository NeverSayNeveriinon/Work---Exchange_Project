using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
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

    public async Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest, ClaimsPrincipal userClaims)
    {
        // 'CurrencyAccountAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,"The 'CurrencyAccountAddRequest' object parameter is Null");
        
        var currency = await _currencyService.GetCurrencyByCurrencyType(currencyAccountAddRequest.CurrencyType);
        var user = await _userManager.GetUserAsync(userClaims);
        
        var currencyAccount = currencyAccountAddRequest.ToCurrencyAccount(user!.Id, currency!.Id);
        
        var transactionAddRequest = new TransactionDepositAddRequest()
        {
            AccountNumber = currencyAccount.Number,
            Money = new MoneyRequest(){Amount = currencyAccountAddRequest.MoneyToOpenAccount.Amount!.Value, CurrencyType = currencyAccountAddRequest.MoneyToOpenAccount.CurrencyType}
        };
        var (isValid, message, _) = await _transactionService.Value.AddDepositTransaction(transactionAddRequest, userClaims);
        if (!isValid)
            return (false, message, null);
        
        var currencyAccountReturned = await _accountRepository.AddCurrencyAccountAsync(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        var currencyAccountResponse = currencyAccountReturned.ToCurrencyAccountResponse();
        return (true, null, currencyAccountResponse);
    }   
    
    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts(ClaimsPrincipal userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);

        List<CurrencyAccount> currencyAccounts;
        if (await _userManager.IsInRoleAsync(user!, "Admin"))
            currencyAccounts = await _accountRepository.GetAllCurrencyAccountsAsync();
        else
            currencyAccounts = await _accountRepository.GetAllCurrencyAccountsByUserAsync(user!.Id);
        
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }

    public async Task<(bool isValid, string? message, CurrencyAccountResponse? obj)> GetCurrencyAccountByNumber(string? number, ClaimsPrincipal userClaims)
    {
        // if 'number' is null
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'number' parameter is Null");
        
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
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteCurrencyAccount(string? number, ClaimsPrincipal userClaims)
    {
        // if 'number' is null
        ArgumentNullException.ThrowIfNull(number,"The CurrencyAccount'Number' parameter is Null");

        var currencyAccount = await _accountRepository.GetCurrencyAccountByNumberAsync(number);
        
        // if 'Number' is invalid (doesn't exist)
        if (currencyAccount == null)
            return (false, false, null);
    
        bool result = _accountRepository.DeleteCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return (result, true, "The Deletion Has Not Been Successful");
    }
    
    
    public async Task<CurrencyAccountResponse?> UpdateBalanceAmount(CurrencyAccount? currencyAccount, decimal? amount, Func<decimal,decimal,decimal> calculationFunc)
    {
        // if 'currencyAccount' is null
        ArgumentNullException.ThrowIfNull(currencyAccount, "The 'currencyAccount' parameter is Null");
        
        // if 'amount' is null
        ArgumentNullException.ThrowIfNull(amount, "The 'amount' parameter is Null");
        
        var updatedCurrencyAccount = _accountRepository.UpdateBalanceAmount(currencyAccount, amount.Value, calculationFunc);
        await _accountRepository.SaveChangesAsync();
        
        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }
    
}