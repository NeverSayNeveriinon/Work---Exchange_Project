using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Enums;
using Core.Helpers;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Core.Services;

public class TransactionService : ITransactionService
{
    private const decimal BalanceMinimumUSD = 50M;
    private const decimal BalanceMaxWithdrawPerDay = 10000M;

    private readonly ITransactionRepository _transactionRepository;
    private readonly ICommissionRateService _commissionRateService;
    private readonly ICurrencyAccountService _currencyAccountService;
    private readonly IExchangeValueService _exchangeValueService;
    private readonly UserManager<UserProfile> _userManager;
    private readonly IAccountRepository _accountRepository;

    public TransactionService(ITransactionRepository transactionRepository, ICommissionRateService commissionRateService, ICurrencyAccountService currencyAccountService, IExchangeValueService exchangeValueService, UserManager<UserProfile> userManager, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _commissionRateService = commissionRateService;
        _currencyAccountService = currencyAccountService;
        _exchangeValueService = exchangeValueService;
        _userManager = userManager;
        _accountRepository = accountRepository;
    }
    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        _accountRepository.LoadReferences(user);        
        // if 'FromAccountNumber' and 'ToAccountNumber' be Same
        if (transactionAddRequest.FromAccountNumber == transactionAddRequest.ToAccountNumber)
            return (false, "'FromAccountNumber' and 'ToAccountNumber' can't be Same", null);
        
        // 'FromAccountNumber' has to belong to the user itself //
        if (!user!.CurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.FromAccountNumber))
            return (false, "'FromAccountNumber' is Not One of Your Accounts Number", null);
        
        // 'ToAccountNumber' has to be one of the user DefinedAccounts Number //
        if (!user!.DefinedCurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.ToAccountNumber))
            return (false, "'ToAccountNumber' is Not One of Your DefinedAccounts Number", null);

        // if 'ToAccountNumber' belongs to the user itself, then the commission is free 
        bool isCommissionFree = false;
        if (user!.CurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.ToAccountNumber))
            isCommissionFree = true;
        
        
        var transaction = transactionAddRequest.ToTransaction();
        var (isFromValid, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.FromAccountNumber);
        if (!isFromValid)
            return (false, "An Account for from account With This Number Has Not Been Found", null);
        var (isToValid, _, toAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.ToAccountNumber);
        if (!isToValid)
            return (false, "An Account for from account With This Number Has Not Been Found", null);
        
        
        var transactionAmount = transactionAddRequest.Amount;
        
        // Calculate Commission to be subtracted from 'Balance' of 'FromAccount'
        decimal amountCommission = 0;
        decimal? cRate = null;
        if (!isCommissionFree)
        {
            var money = new Money() { Amount = transactionAmount!.Value, Currency = fromAccount!.Currency!};
            
            cRate = await _commissionRateService.GetUSDAmountCRate(money);
            if (cRate == null)
                return (false, "There is No Relevant Commission Rate for the Amount", null);
                
            amountCommission = transactionAmount.Value * cRate.GetValueOrDefault();
        }
        
        // Calculate Final Amount to be subtracted from 'Balance' of 'FromAccount'
        var decreaseAmount = transactionAmount + amountCommission;
        var finalAmount = fromAccount!.Balance - decreaseAmount;
        var currencyType = fromAccount.Currency?.CurrencyType;
        var (isMinUSDValid, minUSDMessage) = await CheckMinimumUSDBalanceAsync(currencyType!, finalAmount!.Value);
        if (!isMinUSDValid)
            return (false, minUSDMessage, null); 
        var (isMaxUSDDayValid, maxUSDDayMessage) = await CheckMaxUSDBalanceWithdrawPerDayAsync(currencyType!, decreaseAmount.GetValueOrDefault(), fromAccount, transaction.DateTime);
        if (!isMaxUSDDayValid)
            return (false, maxUSDDayMessage, null);
        
        
        // Calculate Final Amount to be added to 'Balance' of 'ToAccount'
        var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyType(fromAccount.Currency?.CurrencyType, toAccount?.Currency?.CurrencyType);
        if (!isExchangeValid)
            return (false, $"There is No Relevant Exchange Value to convert", null);
        var destinationAmount = transactionAmount * valueToBeMultiplied!.Value;
        
        
        // Updating the 'Balance' of 'FromAccount'
        await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transactionAmount, (val1, val2) => val1 - val2);
        // fromAccount.Balance = fromAccount.Balance - transactionAmount.Value;
        if (!isCommissionFree) await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, amountCommission, (val1, val2) => val1 - val2);

        // Updating the 'Balance' of 'ToAccount'
        await _currencyAccountService.UpdateStashBalanceAmount(toAccount, destinationAmount, (val1, val2) => val1 + val2);

        transaction.CRate = cRate.GetValueOrDefault();
        transaction.FromAccountChangeAmount = transactionAmount.GetValueOrDefault() + amountCommission;
        transaction.ToAccountChangeAmount = destinationAmount.GetValueOrDefault();
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 2)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        var transactionResponse = transactionAddReturned.ToTransactionResponse(valueToBeMultiplied.GetValueOrDefault());
        return (true, null, transactionResponse);
    }
    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest,
                                                                                                       ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        var transaction = transactionAddRequest.ToTransaction();

        // 'AccountNumber' has to belong to the user itself //
        var user = await _userManager.GetUserAsync(userClaims);
        _accountRepository.LoadReferences(user);        

        if (!user!.CurrencyAccounts!.Any(account => account.Number == transactionAddRequest.AccountNumber))
            return (false, "'AccountNumber' is Not One of Your Accounts Number", null);
        
        var (isFromValid, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.AccountNumber);
        if (!isFromValid)
            return (false, "An Account With This Number Has Not Been Found", null);

        var finalAmount = fromAccount.Balance + transactionAddRequest.Money.Amount!.Value;
        var (isMinimumValid, minimumMessage) = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, finalAmount);
        if (!isMinimumValid)
            return (false, minimumMessage, null);
        
        
        // Calculate Amount to be added to 'Balance' of 'Account'
        var toCurrencyType = fromAccount?.Currency?.CurrencyType;
        var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyType(transactionAddRequest.Money.CurrencyType, toCurrencyType);
        if (!isExchangeValid)
            return (false, $"There is No Relevant Exchange Value to convert to {toCurrencyType}", null);
        var amount = transactionAddRequest.Money.Amount * valueToBeMultiplied!.Value;

        // Updating the 'Balance' of 'Account'
        await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, amount, (val1, val2) => val1 + val2);
        
        transaction.FromAccountChangeAmount = amount.GetValueOrDefault();
        transaction.ToAccountChangeAmount = 0;
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again", null);
        
        var transactionResponse = transactionAddReturned.ToTransactionResponse(valueToBeMultiplied.GetValueOrDefault(), transactionAddRequest.Money.CurrencyType);
        return (true, null, transactionResponse);    
    }

    public async Task<(bool isValid, string? message)> CheckMinimumUSDBalanceAsync(string currencyType, decimal finalAmount)
    {
        var (isValid, exchnageValueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyType(currencyType, Constants.USDCurrency);
        if (!isValid)
            return (false, "There is No Relevant Exchange Value to convert to USD");
        
        var finalUSDAmount = finalAmount * exchnageValueToBeMultiplied!.Value;
        if (finalUSDAmount < BalanceMinimumUSD)
            return (false, "The Balance Amount is under 50 USD Dollars, This is Invalid");
        
        return (true, null);
    }
    
    public async Task<(bool isValid, string? message)> CheckMaxUSDBalanceWithdrawPerDayAsync(string currencyType, decimal amount, CurrencyAccount currencyAccount, DateTime transactionDate)
    {
        var (isValid, exchnageValueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyType(currencyType, Constants.USDCurrency);
        if (!isValid)
            return (false, "There is No Relevant Exchange Value to convert to USD");

        var userAllTransactions = await _transactionRepository.GetAllTransactionsByUserAsync(currencyAccount.OwnerID);
        var userAllFromTransactions = userAllTransactions.Where(transaction => transaction.FromAccountNumber == currencyAccount.Number).ToList();
        var userAllFromTransactionsPerDay = userAllFromTransactions.GroupBy(transaction => transaction.DateTime.Date).ToList();
        var userAllFromTransactionsToday = userAllFromTransactionsPerDay.FirstOrDefault(group => group.Key == transactionDate.Date);
        if (userAllFromTransactionsToday == null)
            return (true, null);
        var finalAmount = userAllFromTransactionsToday.Where(t => t.TransactionType == TransactionTypeOptions.Transfer).Sum(t => t.FromAccountChangeAmount);
        
        var finalUSDAmount = finalAmount * exchnageValueToBeMultiplied!.Value;
        if ((finalUSDAmount + amount) > BalanceMaxWithdrawPerDay)
            return (false, "With This Transaction,The Balance Amount Deposit for Today is more than 1000 USD Dollars, This is Invalid");
        
        return (true, null);
    }

    // public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddOpenAccountTransaction(TransactionDepositAddRequest? transactionAddRequest)
    // {
    //     ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
    //     
    //     var transaction = transactionAddRequest.ToTransaction();
    //     var (_,_,currencyAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.AccountNumber);
    //     
    //     // Calculate Amount to be added to 'Balance' of 'Account'
    //     string? toCurrencyType = currencyAccount?.Currency?.CurrencyType;
    //     var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyType(transactionAddRequest.Money.CurrencyType, toCurrencyType);
    //     if (!isExchangeValid)
    //         return (false, $"There is No Relevant Exchange Value to convert to {toCurrencyType}", null);
    //     var amount = transactionAddRequest.Money.Amount * valueToBeMultiplied!.Value;
    //
    //     transaction.FromAccountChangeAmount = amount.GetValueOrDefault();
    //     transaction.ToAccountChangeAmount = 0;
    //     var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
    //     await _transactionRepository.SaveChangesAsync();
    //       
    //     await _currencyAccountService.UpdateStashBalanceAmount(currencyAccount, amount, (val1, val2) => val1 + val2);
    //     
    //     return (true, null, transactionAddReturned.ToTransactionResponse(valueToBeMultiplied.GetValueOrDefault(), transactionAddRequest.Money.CurrencyType));
    // }

    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddOpenAccountDepositTransaction(TransactionDepositAddRequest? transactionAddRequest, CurrencyAccountAddRequest currencyAccountAddRequest)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest, "The 'currencyAccountAddRequest' object parameter is Null");
        
        var transaction = transactionAddRequest.ToTransaction();
        
        // Calculate Amount to be added to 'Balance' of 'Account'
        var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyType(transactionAddRequest.Money.CurrencyType, currencyAccountAddRequest.CurrencyType);
        if (!isExchangeValid)
            return (false, $"There is No Relevant Exchange Value to convert to {currencyAccountAddRequest.CurrencyType}", null);
    
        var (isMinimumValid, minimumMessage) = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, transactionAddRequest.Money.Amount.GetValueOrDefault());
        if (!isMinimumValid)
            return (false, minimumMessage, null);
        
        var finalAmount = transactionAddRequest.Money.Amount * valueToBeMultiplied!.Value;
        transaction.FromAccountChangeAmount = finalAmount.GetValueOrDefault();
        transaction.ToAccountChangeAmount = 0;
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        // await _transactionRepository.SaveChangesAsync();
        
        // await _currencyAccountService.UpdateStashBalanceAmount(transaction.FromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
        
        return (true, null, transactionAddReturned.ToTransactionResponse(valueToBeMultiplied.GetValueOrDefault(), transactionAddRequest.Money.CurrencyType));
    }
    
    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> UpdateTransactionStatusOfTransaction(ConfirmTransactionRequest? confirmTransactionRequest, ClaimsPrincipal userClaims, DateTime DateTimeNow)
    {
        ArgumentNullException.ThrowIfNull(confirmTransactionRequest,"The 'confirmTransactionRequest' parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        var transaction = await _transactionRepository.GetTransactionByIDAsync(confirmTransactionRequest.TransactionId, ignoreQueryFilter:true);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null)
            return (false, null, null);
        
        // In case Someone Else wants to Confirm transaction (and is not admin also)
        if (transaction.FromAccount!.OwnerID != user!.Id || userClaims.IsInRole(Constants.AdminRole))
            return (false, "You Are Not Allowed to Confirm This Transaction", null);
        
        // if 'transactionStatus' is already Confirmed
        if (transaction.TransactionStatus == TransactionStatusOptions.Confirmed)
            return (false, "Transaction is Already Confirmed", null);
        
        // if 'transactionStatus' is already Cancelled
        if (transaction.TransactionStatus == TransactionStatusOptions.Cancelled)
            return (false, "Transaction is Already Cancelled", null);
        
        // if 'transactionStatus' is already Cancelled
        if (transaction.TransactionStatus == TransactionStatusOptions.Failed)
            return (false, "Transaction is Failed Before and Can't Be Confirmed/Cancelled", null);
        
        
        var (_, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transaction.FromAccountNumber);
        
        // if More Than 10 minutes has passed since the transaction time
        if (DateTimeNow.Subtract(transaction.DateTime) > TimeSpan.FromMinutes(10))
        {
            _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Failed);
            if (transaction.TransactionType == TransactionTypeOptions.Deposit)
            {
                await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);
            }
            else if (transaction.TransactionType == TransactionTypeOptions.Transfer)
            {
                var (_, _, toAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transaction.ToAccountNumber);
                await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
                await _currencyAccountService.UpdateStashBalanceAmount(toAccount, transaction.ToAccountChangeAmount, (val1, val2) => val1 - val2);
            }
            var numberOfRowsAffectedFailed = await _transactionRepository.SaveChangesAsync();
            if(numberOfRowsAffectedFailed >= 0) return (false, "The Request Has Not Been Done Completely, Try Again", null);
            return (false, "Invalid, More Than 10 minutes has passed since the transaction time, try another transaction", null);
        }

        var transactionStatus = (TransactionStatusOptions)Enum.Parse(typeof(TransactionStatusOptions), confirmTransactionRequest.TransactionStatus);        
        var updatedTransaction = _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, transactionStatus);

        if (confirmTransactionRequest.TransactionStatus == nameof(TransactionStatusOptions.Confirmed))
        {
            if (transaction.TransactionType == TransactionTypeOptions.Deposit)
            {
                await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);
                await _currencyAccountService.UpdateBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
            }
            else if (transaction.TransactionType == TransactionTypeOptions.Transfer)
            {
                await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
                await _currencyAccountService.UpdateBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);

                var (_, _, toAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transaction.ToAccountNumber);
                await _currencyAccountService.UpdateStashBalanceAmount(toAccount, transaction.ToAccountChangeAmount, (val1, val2) => val1 - val2);
                await _currencyAccountService.UpdateBalanceAmount(toAccount, transaction.ToAccountChangeAmount, (val1, val2) => val1 + val2);
            }
        }
        else if (confirmTransactionRequest.TransactionStatus == nameof(TransactionStatusOptions.Cancelled))
        {
            if (transaction.TransactionType == TransactionTypeOptions.Deposit)
            {
                await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);
            }
            else if (transaction.TransactionType == TransactionTypeOptions.Transfer)
            {
                var (_, _, toAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transaction.ToAccountNumber);
                await _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
                await _currencyAccountService.UpdateStashBalanceAmount(toAccount, transaction.ToAccountChangeAmount, (val1, val2) => val1 - val2);
            }
        }
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if ((transaction.TransactionType == TransactionTypeOptions.Deposit && !(numberOfRowsAffected >= 1)) ||
            (transaction.TransactionType == TransactionTypeOptions.Transfer && !(numberOfRowsAffected >= 2))) 
            return (false, "The Request Has Not Been Done Completely, Try Again", null);

        var transactionResponse = updatedTransaction.ToTransactionResponse();
        return (true, null, transactionResponse);      
    }
    
    
    public async Task<List<TransactionResponse>> GetAllTransactions(ClaimsPrincipal userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);

        List<Transaction> transactions;
        if (await _userManager.IsInRoleAsync(user!, Constants.AdminRole))
            return await GetAllTransactionsInternal();
        else
            transactions = await _transactionRepository.GetAllTransactionsByUserAsync(user!.Id);
        
        var transactionResponses = transactions.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }
    
    public async Task<List<TransactionResponse>> GetAllTransactionsInternal()
    {
        var currencyAccounts = await _transactionRepository.GetAllTransactionsAsync(ignoreQueryFilter:true);
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return currencyAccountResponses;
    }

    public async Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByID(Guid? Id, ClaimsPrincipal userClaims, bool ignoreQueryFilter = false)
    {
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'Id' parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        
        if (await _userManager.IsInRoleAsync(user!, Constants.AdminRole))
            return await GetTransactionByIDInternal(Id);

        var transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value, ignoreQueryFilter);
        
        // if 'id' doesn't exist in 'transactions list' 
        if (transaction == null)
            return (false, null, null);

        if (transaction.FromAccount!.OwnerID != user!.Id && transaction.ToAccount!.OwnerID != user!.Id)
            return (false, "This Transaction Doesn't Belong To You", null);
        
        var transactionResponse = transaction.ToTransactionResponse();
        return (true, null, transactionResponse);
    }

    public async Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByIDInternal(Guid? Id)
    {
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'Id' parameter is Null");
        
        var currencyAccount = await _transactionRepository.GetTransactionByIDAsync(Id.Value, ignoreQueryFilter:true);
        
        // if 'number' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null) 
            return (false, null, null);

        var currencyAccountResponse = currencyAccount.ToTransactionResponse();
        return (true, null, currencyAccountResponse);
    }  
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteTransactionById(Guid? Id, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'ID' parameter is Null");

        var transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);
        
        var user = await _userManager.GetUserAsync(userClaims);
        
        if (await _userManager.IsInRoleAsync(user!, Constants.AdminRole))
            return await DeleteTransactionByIdInternal(Id);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null) 
            return (false, false, null);
        
        if (transaction.FromAccount!.OwnerID != user!.Id)
            return (false, true, "This Transaction Doesn't Belong To You");
    
        _transactionRepository.DeleteTransaction(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, "The Deletion Has Not Be b en Successful");
    }
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteTransactionByIdInternal(Guid? Id)
    {
        ArgumentNullException.ThrowIfNull(Id,"The 'Id' parameter is Null");

        var currencyAccount = await _transactionRepository.GetTransactionByIDAsync(Id.Value, ignoreQueryFilter:true);
        
        // if 'number' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null) 
            return (false, false, null);
        
        _transactionRepository.DeleteTransaction(currencyAccount);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, null);
    }
}