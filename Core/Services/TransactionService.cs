using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.ServicesDTO;
using Core.DTO.ServicesDTO.Money;
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
    private static readonly TimeSpan ChangeStatusMaximumTimeOut = TimeSpan.FromMinutes(10);
    
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICommissionRateService _commissionRateService;
    private readonly ICurrencyAccountService _currencyAccountService;
    private readonly IExchangeValueService _exchangeValueService;
    private readonly UserManager<UserProfile> _userManager;
    private readonly IAccountRepository _accountRepository;

    public TransactionService(ITransactionRepository transactionRepository, ICommissionRateService commissionRateService, 
                              ICurrencyAccountService currencyAccountService, IExchangeValueService exchangeValueService, 
                              UserManager<UserProfile> userManager, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _commissionRateService = commissionRateService;
        _currencyAccountService = currencyAccountService;
        _exchangeValueService = exchangeValueService;
        _userManager = userManager;
        _accountRepository = accountRepository;
    }
    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddTransferTransaction(TransactionTransferAddRequest transactionAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist
        _accountRepository.LoadReferences(user);

        // Check Access to 'From' or 'To' Account  
        var (isAccessValid, accessMessage) = CheckAccountAccess(transactionAddRequest, user);
        if (!isAccessValid) return (false, accessMessage, null);
        
        // if 'ToAccountNumber' belongs to the user itself, then the commission is free 
        bool isCommissionFree = false;
        if (user!.CurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.ToAccountNumber))
            isCommissionFree = true;
        
        
        var transaction = transactionAddRequest.ToTransaction();
        var (isFromValid, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.FromAccountNumber);
        if (!isFromValid) return (false, "An Account for 'from' account With This Number Has Not Been Found", null);
        var (isToValid, _, toAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.ToAccountNumber);
        if (!isToValid) return (false, "An Account for 'to' account With This Number Has Not Been Found", null);
        
        var transactionAmount = transactionAddRequest.Amount!.Value;
       
        // All Calculations and Check Invalid Balance
        var (isValid, message, calculationsAmounts) = await CalculateTransferAmounts(fromAccount!, toAccount!, transaction, isCommissionFree);
        if (!isValid) return (false, message, null);
        var (cRate, commissionAmount, destinationAmount, valueToBeMultiplied) = calculationsAmounts!.ToTuple();
        
        // Updating the 'StashBalance' of 'FromAccount'
        _currencyAccountService.UpdateStashBalanceAmount(fromAccount!, transactionAmount, (val1, val2) => val1 - val2);
        if (!isCommissionFree) _currencyAccountService.UpdateStashBalanceAmount(fromAccount!, commissionAmount, (val1, val2) => val1 - val2);

        // Updating the 'StashBalance' of 'ToAccount'
        _currencyAccountService.UpdateStashBalanceAmount(toAccount!, destinationAmount, (val1, val2) => val1 + val2);
        
        transaction.CRate = cRate;
        transaction.FromAccountChangeAmount = transactionAmount + commissionAmount;
        transaction.ToAccountChangeAmount = destinationAmount;
        
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected >= 3)) return (false, "The Request Has Not Been Done Completely, Try Again", null);

        return (true, null, transactionAddReturned.ToTransactionResponse(valueToBeMultiplied));
    }

    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddDepositTransaction(TransactionDepositAddRequest transactionAddRequest,
                                                                                                       ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        var transaction = transactionAddRequest.ToTransaction();

        // 'AccountNumber' has to belong to the user itself //
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist
        _accountRepository.LoadReferences(user);        

        if (!user.CurrencyAccounts!.Any(account => account.Number == transactionAddRequest.AccountNumber))
            return (false, "'AccountNumber' is Not One of Your Accounts Number", null);
        
        var (isFromValid, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.AccountNumber);
        if (!isFromValid) return (false, "An Account With This Number Has Not Been Found", null);

        var finalAmount = fromAccount!.Balance + transactionAddRequest.Money.Amount!.Value;
        var (isMinimumValid, minimumMessage) = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, finalAmount);
        if (!isMinimumValid) return (false, minimumMessage, null);
        
        
        // Calculate Amount to be added to 'StashBalance' of 'Account'
        var toCurrencyType = fromAccount.Currency?.CurrencyType;
        var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyTypes(transactionAddRequest.Money.CurrencyType, toCurrencyType!);
        if (!isExchangeValid) return (false, $"There is No Relevant Exchange Value to convert to {toCurrencyType}", null);
        var amount = transactionAddRequest.Money.Amount * valueToBeMultiplied!.Value;

        // Updating the 'StashBalance' of 'Account'
        _currencyAccountService.UpdateStashBalanceAmount(fromAccount!, amount.GetValueOrDefault(), (val1, val2) => val1 + val2);
        
        transaction.FromAccountChangeAmount = amount.GetValueOrDefault();
        transaction.ToAccountChangeAmount = 0;
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, "The Request Has Not Been Done Completely, Try Again", null);
        
        var transactionResponse = transactionAddReturned.ToTransactionResponse(valueToBeMultiplied.GetValueOrDefault(), transactionAddRequest.Money.CurrencyType);
        return (true, null, transactionResponse);    
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

    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> AddOpenAccountDepositTransaction(TransactionDepositAddRequest transactionAddRequest, CurrencyAccountAddRequest currencyAccountAddRequest)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest, $"The '{nameof(currencyAccountAddRequest)}' object parameter is Null");
        
        var transaction = transactionAddRequest.ToTransaction();
        
        // Calculate Amount to be added to 'Balance' of 'Account'
        var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyTypes(transactionAddRequest.Money.CurrencyType, currencyAccountAddRequest.CurrencyType);
        if (!isExchangeValid)
            return (false, $"There is No Relevant Exchange Value to convert to {currencyAccountAddRequest.CurrencyType}", null);
    
        var (isMinimumValid, minimumMessage) = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, transactionAddRequest.Money.Amount.GetValueOrDefault());
        if (!isMinimumValid) return (false, minimumMessage, null);
        
        var finalAmount = transactionAddRequest.Money.Amount * valueToBeMultiplied!.Value;
        transaction.FromAccountChangeAmount = finalAmount.GetValueOrDefault();
        transaction.ToAccountChangeAmount = 0;
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);

        var transactionResponse = transactionAddReturned.ToTransactionResponse(valueToBeMultiplied.GetValueOrDefault(), transactionAddRequest.Money.CurrencyType);
        return (true, null, transactionResponse);
    }
    
    public async Task<(bool isValid, string? message, TransactionResponse? obj)> UpdateTransactionStatusOfTransaction(ConfirmTransactionRequest confirmTransactionRequest, ClaimsPrincipal userClaims, DateTime DateTimeNow)
    {
        ArgumentNullException.ThrowIfNull(confirmTransactionRequest,$"The '{nameof(confirmTransactionRequest)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist
        
        var transaction = await _transactionRepository.GetTransactionByIDAsync(confirmTransactionRequest.TransactionId, ignoreQueryFilter:true);
        if (transaction == null) return (false, null, null); // if 'ID' is invalid (doesn't exist)
        var fromAccount = transaction.FromAccount;
        var toAccount = transaction.ToAccount;
        
        // In case Someone Else wants to Confirm transaction (and is not admin also)
        if (transaction.FromAccount!.OwnerID != user!.Id && !userClaims.IsInRole(Constants.AdminRole))
            return (false, "You Are Not Allowed to Confirm/Cancel This Transaction", null);

        // if 'transactionStatus' from db is other than "Pending"
        var invalidTransactionStatus = CheckInvalidTransactionStatus(transaction.TransactionStatus);
        if (!invalidTransactionStatus.isValid) return (false, invalidTransactionStatus.message, null);
        
        // if More Than 10 minutes has passed since the transaction time
        if (DateTimeNow.Subtract(transaction.DateTime) > ChangeStatusMaximumTimeOut)
        {
            _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Failed);
            DiscardTransactionFee(transaction, fromAccount!, toAccount!);
            
            var numberOfRowsAffectedFailed = await _transactionRepository.SaveChangesAsync();
            if ((transaction.TransactionType == TransactionTypeOptions.Deposit && !(numberOfRowsAffectedFailed >= 1)) ||
                (transaction.TransactionType == TransactionTypeOptions.Transfer && !(numberOfRowsAffectedFailed >= 2))) 
                return (false, "The Request Has Not Been Done Completely, Try Again", null);

            return (false, "Invalid, More Than 10 minutes has passed since the transaction time, try another transaction", null);
        }

        // if 'transactionStatus' from db is "Pending"
        Transaction? updatedTransaction = null;
        if (confirmTransactionRequest.TransactionStatus == nameof(TransactionStatusOptions.Confirmed))
        {
            updatedTransaction = _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Confirmed);
            var (isValid, message) = await ApplyTransactionFee(transaction, fromAccount!, toAccount!);
            if (!isValid) return (false, message, null);
        }
        else if (confirmTransactionRequest.TransactionStatus == nameof(TransactionStatusOptions.Cancelled))
        {
            updatedTransaction = _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Cancelled);
            DiscardTransactionFee(transaction, fromAccount!, toAccount!);
        }
        
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if ((transaction.TransactionType == TransactionTypeOptions.Deposit && !(numberOfRowsAffected >= 2)) ||
            (transaction.TransactionType == TransactionTypeOptions.Transfer && !(numberOfRowsAffected >= 3))) 
            return (false, "The Request Has Not Been Done Completely, Try Again", null);

        return (true, null, updatedTransaction?.ToTransactionResponse());      
    }
    
    public async Task<List<TransactionResponse>> GetAllTransactions(ClaimsPrincipal userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);
        // if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist

        if (await _userManager.IsInRoleAsync(user!, Constants.AdminRole))
            return await GetAllTransactionsInternal();
        
        var transactions = await _transactionRepository.GetAllTransactionsByUserAsync(user!.Id);
        
        var transactionResponses = transactions.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }
    
    public async Task<List<TransactionResponse>> GetAllTransactionsInternal()
    {
        var currencyAccounts = await _transactionRepository.GetAllTransactionsAsync(ignoreQueryFilter:true);
        var currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return currencyAccountResponses;
    }

    public async Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByID(Guid id, ClaimsPrincipal userClaims, bool ignoreQueryFilter = false)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, "The User Doesn't Exist", null); // if 'user' doesn't exist

        if (await _userManager.IsInRoleAsync(user, Constants.AdminRole))
            return await GetTransactionByIDInternal(id);

        var transaction = await _transactionRepository.GetTransactionByIDAsync(id, ignoreQueryFilter);
        if (transaction == null) return (false, null, null); // if 'id' doesn't exist in 'transactions list'

        if (transaction.FromAccount!.OwnerID != user!.Id && transaction.ToAccount!.OwnerID != user!.Id)
            return (false, "This Transaction Doesn't Belong To You", null);
        
        return (true, null, transaction.ToTransactionResponse());
    }

    public async Task<(bool isValid, string? message, TransactionResponse? obj)> GetTransactionByIDInternal(Guid id)
    {
        var currencyAccount = await _transactionRepository.GetTransactionByIDAsync(id, ignoreQueryFilter:true);
        if (currencyAccount == null) return (false, null, null); // if 'number' doesn't exist in 'currencyAccounts list' 

        return (true, null, currencyAccount.ToTransactionResponse());
    }  
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteTransactionById(Guid id, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return (false, true, "The User Doesn't Exist"); // if 'user' doesn't exist

        if (await _userManager.IsInRoleAsync(user, Constants.AdminRole))
            return await DeleteTransactionByIdInternal(id);
        
        var transaction = await _transactionRepository.GetTransactionByIDAsync(id);
        if (transaction == null) return (false, false, null); // if 'ID' is invalid (doesn't exist)
        
        if (transaction.FromAccount!.OwnerID != user.Id)
            return (false, true, "This Transaction Doesn't Belong To You");
    
        _transactionRepository.DeleteTransaction(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, "The Deletion Has Not Be b en Successful");
    }
    
    public async Task<(bool isValid, bool isFound, string? message)> DeleteTransactionByIdInternal(Guid id)
    {
        var currencyAccount = await _transactionRepository.GetTransactionByIDAsync(id, ignoreQueryFilter:true);
        if (currencyAccount == null) return (false, false, null); // if 'number' doesn't exist in 'currencyAccounts list' 
        
        _transactionRepository.DeleteTransaction(currencyAccount);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return (false, true, "The Request Has Not Been Done Completely, Try Again");

        return (true, true, null);
    }
    
    
    
    
    
    
    
    // Private Methods //
    
    private (bool isValid, string? message) CheckAccountAccess(TransactionTransferAddRequest transactionAddRequest, UserProfile user)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(user,$"The '{nameof(user)}' object parameter is Null");

        // if 'FromAccountNumber' and 'ToAccountNumber' be Same
        if (transactionAddRequest.FromAccountNumber == transactionAddRequest.ToAccountNumber)
            return (false, "'FromAccountNumber' and 'ToAccountNumber' can't be Same");
        
        // 'FromAccountNumber' has to belong to the user itself //
        if (!user.CurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.FromAccountNumber))
            return (false, "'FromAccountNumber' is Not One of Your Accounts Number");
        
        // 'ToAccountNumber' has to be one of the user DefinedAccounts Number //
        if (!user.DefinedCurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.ToAccountNumber))
            return (false, "'ToAccountNumber' is Not One of Your DefinedAccounts Number");

        return (true, null);
    }
    
    private async Task<(bool isValid, string? message, TransferCalculationsAmounts? obj)> CalculateTransferAmounts(CurrencyAccount fromAccount, CurrencyAccount toAccount,
                                                                                                                   Transaction transaction, bool isCommissionFree)
    {
        // Calculate Commission to be subtracted from 'StashBalance' of 'FromAccount'
        decimal commissionAmount = 0;
        decimal? cRate = null;
        if (!isCommissionFree)
        {
            var money = new Money() { Amount = transaction.Amount, Currency = fromAccount.Currency!};
            cRate = await _commissionRateService.GetUSDAmountCRate(money);
            if (cRate == null) return (false, "There is No Relevant Commission Rate for the Amount", null);
                
            commissionAmount = transaction.Amount * cRate.GetValueOrDefault();
        }
        
        // Calculate Final Amount to be subtracted from 'StashBalance' of 'FromAccount'
        var decreaseAmount = transaction.Amount + commissionAmount;
        var currencyType = fromAccount.Currency?.CurrencyType;

        var (isBalanceValid, balanceMessage) = await CheckFromAccountInvalidBalance(fromAccount, currencyType!, decreaseAmount, transaction.DateTime);
        if (!isBalanceValid) return (false, balanceMessage, null);
        
        // Calculate Final Amount to be added to 'StashBalance' of 'ToAccount'
        var (isExchangeValid, valueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyTypes(fromAccount.Currency?.CurrencyType!, toAccount.Currency?.CurrencyType!);
        if (!isExchangeValid) return (false, $"There is No Relevant Exchange Value to convert to {toAccount.Currency?.CurrencyType!}", null);
        var destinationAmount = transaction.Amount * valueToBeMultiplied!.Value;

        
        var transactionAmount = new TransferCalculationsAmounts()
        {
            CRate = cRate!.Value,
            CommissionAmount = commissionAmount,
            DestinationAmount = destinationAmount,
            ValueToBeMultiplied = valueToBeMultiplied.Value
        };
        return (true, null, transactionAmount);
    }

    private async Task<(bool isValid, string? message)> CheckFromAccountInvalidBalance(CurrencyAccount fromAccount, string currencyType,
                                                                                       decimal decreaseAmount, DateTime dateTime)
    {
        var finalBalanceAmount = fromAccount.Balance - decreaseAmount;

        var (isMinUSDValid, minUSDMessage) = await CheckMinimumUSDBalanceAsync(currencyType, finalBalanceAmount);
        if (!isMinUSDValid) return (false, minUSDMessage); 
        
        var (isMaxUSDDayValid, maxUSDDayMessage) = await CheckMaxUSDBalanceWithdrawPerDayAsync(currencyType, decreaseAmount, fromAccount, dateTime);
        if (!isMaxUSDDayValid) return (false, maxUSDDayMessage);

        return (true, null);
    }
    
    private async Task<(bool isValid, string? message)> CheckMinimumUSDBalanceAsync(string currencyType, decimal finalAmount)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' object parameter is Null");

        var (isValid, exchnageValueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyTypes(currencyType, Constants.USDCurrency);
        if (!isValid) return (false, "There is No Relevant Exchange Value to convert to USD");
        
        var finalUSDAmount = finalAmount * exchnageValueToBeMultiplied!.Value;
        if (finalUSDAmount < BalanceMinimumUSD)
            return (false, "The Balance Amount is under 50 USD Dollars, This is Invalid");
        
        return (true, null);
    }
    
    private async Task<(bool isValid, string? message)> CheckMaxUSDBalanceWithdrawPerDayAsync(string currencyType, decimal amount, CurrencyAccount currencyAccount, DateTime transactionDate)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccount,$"The '{nameof(currencyAccount)}' object parameter is Null");

        var (isValid, exchnageValueToBeMultiplied) = await _exchangeValueService.GetExchangeValueByCurrencyTypes(currencyType, Constants.USDCurrency);
        if (!isValid) return (false, "There is No Relevant Exchange Value to convert to USD");

        var userAllTransactions = await _transactionRepository.GetAllTransactionsByUserAsync(currencyAccount.OwnerID);
        var userAllFromTransactionsPerDay = userAllTransactions.Where(t => t.FromAccountNumber == currencyAccount.Number &&
                                                                           t.TransactionType == TransactionTypeOptions.Transfer)
                                                               .GroupBy(t => t.DateTime.Date).ToList();
        var userAllFromTransactionsToday = userAllFromTransactionsPerDay.FirstOrDefault(group => group.Key == transactionDate.Date);
        if (userAllFromTransactionsToday == null) return (true, null);
        var finalAmount = userAllFromTransactionsToday.Sum(t => t.FromAccountChangeAmount);
        
        var finalUSDAmount = finalAmount * exchnageValueToBeMultiplied!.Value;
        if ((finalUSDAmount + amount) > BalanceMaxWithdrawPerDay)
            return (false, "With This Transaction,The Balance Amount Deposit for Today is more than 1000 USD Dollars, This is Invalid");
        
        return (true, null);
    }
    
    
    private (bool isValid, string? message) CheckInvalidTransactionStatus(TransactionStatusOptions transactionStatus) => transactionStatus switch
    {
        TransactionStatusOptions.Confirmed => (false, "Transaction is Already Confirmed"),
        TransactionStatusOptions.Cancelled => (false, "Transaction is Already Cancelled"),
        TransactionStatusOptions.Failed => (false, "Transaction is Failed Before and Can't Be Confirmed/Cancelled"),
        _ => (true, null)
    };
    
    private async Task<(bool isValid, string? message)> ApplyTransactionFee(Transaction transaction, CurrencyAccount fromAccount, CurrencyAccount? toAccount)
    {
        ArgumentNullException.ThrowIfNull(transaction, $"The '{nameof(transaction)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(fromAccount, $"The '{nameof(fromAccount)}' object parameter is Null");

        if (transaction.TransactionType == TransactionTypeOptions.Deposit)
        {
            _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);
            _currencyAccountService.UpdateBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
        }
        else if (transaction.TransactionType == TransactionTypeOptions.Transfer)
        {
            // Check 'From Account' Balance (real Balance)
            var decreaseAmount = fromAccount.Balance - transaction.FromAccountChangeAmount;
            
            var (isMinimumValid, minimumMessage) = await CheckMinimumUSDBalanceAsync(fromAccount.Currency!.CurrencyType, decreaseAmount);
            if (!isMinimumValid) return (false, minimumMessage);
            var (isMaxUSDDayValid, maxUSDDayMessage) = await CheckMaxUSDBalanceWithdrawPerDayAsync(fromAccount.Currency!.CurrencyType, decreaseAmount, fromAccount, transaction.DateTime);
            if (!isMaxUSDDayValid) return (false, maxUSDDayMessage);
            
            _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
            _currencyAccountService.UpdateBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);

            _currencyAccountService.UpdateStashBalanceAmount(toAccount!, transaction.ToAccountChangeAmount, (val1, val2) => val1 - val2);
            _currencyAccountService.UpdateBalanceAmount(toAccount!, transaction.ToAccountChangeAmount, (val1, val2) => val1 + val2);
        }

        return (true, null);
    }

    private void DiscardTransactionFee(Transaction transaction, CurrencyAccount fromAccount, CurrencyAccount? toAccount)
    {
        ArgumentNullException.ThrowIfNull(transaction, $"The '{nameof(transaction)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(fromAccount, $"The '{nameof(fromAccount)}' object parameter is Null");
        
        if (transaction.TransactionType == TransactionTypeOptions.Deposit)
        {
            _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);
        }
        else if (transaction.TransactionType == TransactionTypeOptions.Transfer)
        {
            _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
            _currencyAccountService.UpdateStashBalanceAmount(toAccount!, transaction.ToAccountChangeAmount, (val1, val2) => val1 - val2);
        }
    }
    
}