using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.TransactionDTO;
using Core.Enums;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Core.Services;

public class TransactionService : ITransactionService
{
    private static readonly decimal BalanceMinimumUSD = 50M;
    
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICommissionRateService _commissionRateService;
    private readonly ICurrencyAccountService _currencyAccountService;
    private readonly IExchangeValueService _exchangeValueService;
    private readonly UserManager<UserProfile> _userManager;
    
    public TransactionService(ITransactionRepository transactionRepository, ICommissionRateService commissionRateService, ICurrencyAccountService currencyAccountService, IExchangeValueService exchangeValueService, UserManager<UserProfile> userManager)
    {
        _transactionRepository = transactionRepository;
        _commissionRateService = commissionRateService;
        _currencyAccountService = currencyAccountService;
        _exchangeValueService = exchangeValueService;
        _userManager = userManager;
    }
    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest, ClaimsPrincipal userClaims)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        // 'FromAccountNumber' has to belong to the user itself //
        var user = await _userManager.GetUserAsync(userClaims);
        var userAccountsIdSet = user!.CurrencyAccounts!.Select(account => account.Number).ToHashSet();
        if (!userAccountsIdSet.Contains(transactionAddRequest.FromAccountNumber))
            return (false, "'FromAccountNumber' is Not One of Your Accounts Number", null);
        
        // 'ToAccountNumber' has to be one the user DefinedAccounts Number //
        var userDefinedAccountsIdSet = user.DefinedAccountNumbers?.ToHashSet();
        if (!userDefinedAccountsIdSet!.Contains(transactionAddRequest.ToAccountNumber))
            return (false, "'ToAccountNumber' is Not One of Your DefinedAccounts Number", null);

        // if 'ToAccountNumber' belongs to the user itself, then the commission is free 
        bool isCommissionFree = false;
        if (userAccountsIdSet.Contains(transactionAddRequest.ToAccountNumber))
            isCommissionFree = true;
        
        
        var transaction = transactionAddRequest.ToTransaction();
        _transactionRepository.LoadReferences(transaction);
        var transactionAmount = transactionAddRequest.Amount;
        
        // Calculate Commission to be subtracted from 'Balance' of 'FromAccount'
        decimal amountCommission = 0;
        if (!isCommissionFree)
        {
            var money = new Money()
            {
                Amount = transactionAmount!.Value,
                Currency = transaction.FromAccount!.Currency!
            };
            var cRate = await _commissionRateService.GetUSDAmountCRate(money);
            if (cRate == null)
                return (false, "There is No Relevant Commission Rate for the Amount", null);
                
            amountCommission = transactionAmount.Value * cRate.Value;
        }
        
        // Calculate Final Amount to be subtracted from 'Balance' of 'FromAccount'
        var decreaseAmount = transactionAmount + amountCommission;
        var finalAmount = transaction.FromAccount!.Balance - decreaseAmount;
        var currencyType = transaction.FromAccount.Currency == null ? null : Enum.GetName(typeof(CurrencyTypeOptions), transaction.FromAccount.Currency.CurrencyType);
        var (isValid, message) = await CheckMinimumUSDBalanceAsync(currencyType!, finalAmount!.Value);
        if (!isValid)
            return (false, message, null);
        
        
        // Calculate Final Amount to be added to 'Balance' of 'ToAccount'
        var valueToBeMultiplied = transaction.FromAccount?.Currency?.FirstExchangeValues?
            .FirstOrDefault(exValue => exValue.FirstCurrencyId == transaction.ToAccount?.CurrencyID)
            ?.UnitOfFirstValue;
        var destinationAmount = transactionAmount * valueToBeMultiplied!.Value;
        
        
        // Updating the 'Balance' of 'FromAccount'
        await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, transactionAmount, (val1, val2) => val1 - val2);
        if (!isCommissionFree) await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, amountCommission, (val1, val2) => val1 - val2);

        // Updating the 'Balance' of 'ToAccount'
        await _currencyAccountService.UpdateBalanceAmount(transaction.ToAccount, destinationAmount, (val1, val2) => val1 + val2);

        
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        var transactionResponse = transactionAddReturned.ToTransactionResponse();
        return (true, null, transactionResponse);
    }

    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest, ClaimsPrincipal userClaims)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        var transaction = transactionAddRequest.ToTransaction();
        _transactionRepository.LoadReferences(transaction);

        // 'AccountNumber' has to belong to the user itself //
        var user = await _userManager.GetUserAsync(userClaims);
        var userAccountsIdSet = user!.CurrencyAccounts!.Select(account => account.Number).ToHashSet();
        if (!userAccountsIdSet.Contains(transactionAddRequest.AccountNumber))
            return (false, "'AccountNumber' is Not One of Your Accounts Number", null);
        
        var (isValid, message) = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, transactionAddRequest.Money.Amount!.Value);
        if (!isValid)
            return (false, message, null);
        
        // Calculate Amount to be added to 'Balance' of 'Account'
        var moneyCurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), transactionAddRequest.Money.CurrencyType);
        var valueToBeMultiplied = transaction.FromAccount?.Currency?.SecondExchangeValues?.FirstOrDefault(exValue=> exValue.FirstCurrency.CurrencyType == moneyCurrencyType)!.UnitOfFirstValue;
        var amount = transactionAddRequest.Money.Amount * valueToBeMultiplied!.Value;
        
        // Updating the 'Balance' of 'Account'
        await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, amount, (val1, val2) => val1 + val2);
        
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();
        
        var transactionResponse = transactionAddReturned.ToTransactionResponse();
        return (true, null, transactionResponse);    
    }

    private async Task<(bool isValid, string? message)> CheckMinimumUSDBalanceAsync(string currencyType, decimal finalAmount)
    {
        var (isValid, valueToBeMultiplied) = await _exchangeValueService.GetUSDExchangeValueByCurrencyType(currencyType);
        if (!isValid)
            return (false, "Either The Source Currency Type doesn't exist or There is No Relevant Exchange Value to convert to USD");
        
        var finalUSDAmount = finalAmount * valueToBeMultiplied;
        if (finalUSDAmount < BalanceMinimumUSD)
        {
            return (false, "The Balance Amount is under 50 USD Dollars, This is Invalid");
        }
        return (true, null);
    }

    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> UpdateIsConfirmedOfTransaction(int? transactionId, ClaimsPrincipal userClaims, bool? isConfirmed, TimeSpan TimeNow)
    {
        // if 'transactionId' is null
        ArgumentNullException.ThrowIfNull(transactionId,"The 'transactionId' parameter is Null");
        
        // if 'isConfirmed' is null
        ArgumentNullException.ThrowIfNull(isConfirmed,"The 'isConfirmed' parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        var transaction = await _transactionRepository.GetTransactionByIDAsync(transactionId.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null)
            return (false, null, null);
    
        // In case Someone Else wants to Confirm transaction
        if (transaction.FromAccount!.OwnerID != user!.Id)
            return (false, "You Are Not Allowed to Confirm This Transaction", null);
        
        // if More Than 10 minutes has passed since the transaction time
        if (transaction.DateTime.TimeOfDay.Minutes - TimeNow.Minutes > 10)
            return (false, "Invalid, More Than 10 minutes has passed since the transaction time, try another transaction", null);
            
        var updatedTransaction = _transactionRepository.UpdateIsConfirmedOfTransaction(transaction, isConfirmed.Value);
        await _transactionRepository.SaveChangesAsync();

        var transactionResponse = updatedTransaction.ToTransactionResponse();
        return (true, null, transactionResponse);      
    }
    
    
    public async Task<List<TransactionResponse>> GetAllTransactions(ClaimsPrincipal userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);

        List<Transaction> transactions;
        if (await _userManager.IsInRoleAsync(user!, "Admin"))
            transactions = await _transactionRepository.GetAllTransactionsAsync();
        else
            transactions = await _transactionRepository.GetAllTransactionsByUserAsync(user!.Id);
        
        var transactionResponses = transactions.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }

    public async Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByID(int? Id, ClaimsPrincipal userClaims)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'Id' parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        var transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'transactions list' 
        if (transaction == null)
            return (false, null, null);

        if (transaction.FromAccount!.OwnerID != user!.Id || transaction.ToAccount!.OwnerID != user!.Id)
            return (false, "This Transaction Doesn't Belong To You", null);
        
        var transactionResponse = transaction.ToTransactionResponse();
        return (true, null, transactionResponse);
    }

    // public async Task<TransactionResponse?> UpdateTransaction(TransactionUpdateRequest? transactionUpdateRequest, int? transactionID)
    // {
    //     // if 'transaction ID' is null
    //     ArgumentNullException.ThrowIfNull(transactionID,"The Transaction'ID' parameter is Null");
    //     
    //     // if 'transactionUpdateRequest' is null
    //     ArgumentNullException.ThrowIfNull(transactionUpdateRequest,"The 'TransactionUpdateRequest' object parameter is Null");
    //     
    //     var transaction = await _transactionRepository.GetTransactionByIDAsync(transactionID.Value);
    //     
    //     // if 'ID' is invalid (doesn't exist)
    //     if (transaction == null) return null;
    //         
    //     var updatedTransaction = _transactionRepository.UpdateTransaction(transaction, transactionUpdateRequest.ToTransaction());
    //     await _transactionRepository.SaveChangesAsync();
    //
    //     return updatedTransaction.ToTransactionResponse();
    // }

    public async Task<(bool isValid, bool isFound, string? message)> DeleteTransaction(int? Id, ClaimsPrincipal userClaims)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'ID' parameter is Null");

        var transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null) 
            return (false, false, null);

    
        bool result = _transactionRepository.DeleteTransaction(transaction);
        await _transactionRepository.SaveChangesAsync();

        return (result, true, "The Deletion Has Not Been Successful");
    }
}