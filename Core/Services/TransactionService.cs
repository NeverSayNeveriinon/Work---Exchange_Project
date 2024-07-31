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
using FluentResults;
using Microsoft.AspNetCore.Identity;
using static Core.Helpers.FluentResultsExtensions;

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
    private readonly IAccountService _accountService;

    public TransactionService(ITransactionRepository transactionRepository, ICommissionRateService commissionRateService, 
                              ICurrencyAccountService currencyAccountService, IExchangeValueService exchangeValueService, 
                              UserManager<UserProfile> userManager, IAccountService accountService)
    {
        _transactionRepository = transactionRepository;
        _commissionRateService = commissionRateService;
        _currencyAccountService = currencyAccountService;
        _exchangeValueService = exchangeValueService;
        _userManager = userManager;
        _accountService = accountService;
    }
    
    public async Task<Result<TransactionResponse>> AddTransferTransaction(TransactionTransferAddRequest transactionAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist
        _accountService.LoadReferences(user);

        // Check Access to 'From' or 'To' Account  
        var accessValidateResult = CheckAccountAccess(transactionAddRequest, user);
        // if (accessValidateResult.IsFailed) return accessValidateResult.ToResult();
        if (accessValidateResult.IsFailed) return Result.Fail(accessValidateResult.FirstErrorMessage());
        
        // if 'ToAccountNumber' belongs to the user itself, then the commission is free 
        bool isCommissionFree = false;
        if (user!.CurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.ToAccountNumber))
            isCommissionFree = true;
        
        var transaction = transactionAddRequest.ToTransaction();
        var (isFromValid, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.FromAccountNumber);
        if (!isFromValid) return Result.Fail(CreateNotFoundError("An Account for 'from' account With This Number Has Not Been Found"));
        var (isToValid, _, toAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.ToAccountNumber);
        if (!isToValid) return Result.Fail(CreateNotFoundError("An Account for 'to' account With This Number Has Not Been Found"));
        
        var transactionAmount = transactionAddRequest.Amount!.Value;
       
        // All Calculations and Check Invalid Balance
        var (amountsValidateResult, calculationsAmounts) = (await CalculateTransferAmounts(fromAccount!, toAccount!, transaction, isCommissionFree)).DeconstructObject();
        // if (amountsValidateResult.IsFailed) return amountsValidateResult.ToResult();
        if (amountsValidateResult.IsFailed) return Result.Fail(amountsValidateResult.FirstErrorMessage());
        var (cRate, commissionAmount, destinationAmount, valueToBeMultiplied) = calculationsAmounts.ToTuple();
        
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
        if (!(numberOfRowsAffected >= 3)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(transactionAddReturned.ToTransactionResponse(valueToBeMultiplied));
    }

    
    public async Task<Result<TransactionResponse>> AddDepositTransaction(TransactionDepositAddRequest transactionAddRequest, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        var transaction = transactionAddRequest.ToTransaction();

        // 'AccountNumber' has to belong to the user itself //
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist
        _accountService.LoadReferences(user);        

        if (!user.CurrencyAccounts!.Any(account => account.Number == transactionAddRequest.AccountNumber))
             return Result.Fail(CreateNotFoundError("'AccountNumber' is Not One of Your Accounts Number"));
        
        var (isFromValid, _, fromAccount) = await _currencyAccountService.GetCurrencyAccountByNumberWithNavigationInternal(transactionAddRequest.AccountNumber);
        if (!isFromValid) return Result.Fail(CreateNotFoundError("An Account With This Number Has Not Been Found"));

        var finalAmount = fromAccount.Balance + transactionAddRequest.Money.Amount!.Value;
        var minimumUSDValidateResult = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, finalAmount);
        // if (minimumUSDValidateResult.IsFailed) return minimumUSDValidateResult.ToResult();
        if (minimumUSDValidateResult.IsFailed) return Result.Fail(minimumUSDValidateResult.FirstErrorMessage());
        
        // Calculate Amount to be added to 'StashBalance' of 'Account'
        var toCurrencyType = fromAccount.Currency?.CurrencyType;
        var (exchangeValidateResult, valueToBeMultiplied) = (await _exchangeValueService.GetExchangeValueByCurrencyTypes(transactionAddRequest.Money.CurrencyType, toCurrencyType!)).DeconstructObject();
        if (exchangeValidateResult.IsFailed) return Result.Fail($"There is No Relevant Exchange Value to convert to {toCurrencyType}");
        var amount = transactionAddRequest.Money.Amount * valueToBeMultiplied;

        // Updating the 'StashBalance' of 'Account'
        _currencyAccountService.UpdateStashBalanceAmount(fromAccount!, amount.GetValueOrDefault(), (val1, val2) => val1 + val2);
        
        transaction.FromAccountChangeAmount = amount.GetValueOrDefault();
        transaction.ToAccountChangeAmount = 0;
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) Result.Fail("The Request Has Not Been Done Completely, Try Again");
        
        var transactionResponse = transactionAddReturned.ToTransactionResponse(valueToBeMultiplied, transactionAddRequest.Money.CurrencyType);
        return Result.Ok(transactionResponse);    
    }
    
    
    // public async Task<Result<TransactionResponse>> AddOpenAccountTransaction(TransactionDepositAddRequest? transactionAddRequest)
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

    
    public async Task<Result<TransactionResponse>> AddOpenAccountDepositTransaction(TransactionDepositAddRequest transactionAddRequest, CurrencyAccountAddRequest currencyAccountAddRequest)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest, $"The '{nameof(currencyAccountAddRequest)}' object parameter is Null");
        
        var transaction = transactionAddRequest.ToTransaction();
        
        // Calculate Amount to be added to 'Balance' of 'Account'
        var (exchangeValidateResult, valueToBeMultiplied) = (await _exchangeValueService.GetExchangeValueByCurrencyTypes(transactionAddRequest.Money.CurrencyType, currencyAccountAddRequest.CurrencyType)).DeconstructObject();
        if (exchangeValidateResult.IsFailed) return Result.Fail($"There is No Relevant Exchange Value to convert to {currencyAccountAddRequest.CurrencyType}");
    
        var minimumUSDValidateResult = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, transactionAddRequest.Money.Amount.GetValueOrDefault());
        // if (minimumUSDValidateResult.IsFailed) return minimumUSDValidateResult.ToResult();
        if (minimumUSDValidateResult.IsFailed) return Result.Fail(minimumUSDValidateResult.FirstErrorMessage());
        
        var finalAmount = transactionAddRequest.Money.Amount * valueToBeMultiplied;
        transaction.FromAccountChangeAmount = finalAmount.GetValueOrDefault();
        transaction.ToAccountChangeAmount = 0;
        var transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);

        var transactionResponse = transactionAddReturned.ToTransactionResponse(valueToBeMultiplied, transactionAddRequest.Money.CurrencyType);
        return Result.Ok(transactionResponse);    
    }
    
    public async Task<Result<TransactionResponse>> UpdateTransactionStatusOfTransaction(ConfirmTransactionRequest confirmTransactionRequest, ClaimsPrincipal userClaims, DateTime DateTimeNow)
    {
        ArgumentNullException.ThrowIfNull(confirmTransactionRequest,$"The '{nameof(confirmTransactionRequest)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist
        
        var transaction = await _transactionRepository.GetTransactionByIDAsync(confirmTransactionRequest.TransactionId, ignoreQueryFilter:true);
        if (transaction == null) return Result.Fail(CreateNotFoundError("!!A Transaction With This ID Has Not Been Found!!")); // if 'ID' is invalid (doesn't exist)
        var fromAccount = transaction.FromAccount;
        var toAccount = transaction.ToAccount;
        
        // In case Someone Else wants to Confirm transaction (and is not admin also)
        if (transaction.FromAccount!.OwnerID != user!.Id && !userClaims.IsInRole(Constants.AdminRole))
            return Result.Fail("You Are Not Allowed to Confirm/Cancel This Transaction");

        // if 'transactionStatus' from db is other than "Pending"
        var transactionStatusValidateResult = CheckInvalidTransactionStatus(transaction.TransactionStatus);
        // if (transactionStatusValidateResult.IsFailed) return transactionStatusValidateResult.ToResult();
        if (transactionStatusValidateResult.IsFailed) return Result.Fail(transactionStatusValidateResult.FirstErrorMessage());
        
        // if More Than 10 minutes has passed since the transaction time
        if (DateTimeNow.Subtract(transaction.DateTime) > ChangeStatusMaximumTimeOut)
        {
            _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Failed);
            DiscardTransactionFee(transaction, fromAccount!, toAccount!);
            
            var numberOfRowsAffectedFailed = await _transactionRepository.SaveChangesAsync();
            if ((transaction.TransactionType == TransactionTypeOptions.Deposit && !(numberOfRowsAffectedFailed >= 1)) ||
                (transaction.TransactionType == TransactionTypeOptions.Transfer && !(numberOfRowsAffectedFailed >= 2))) 
                return Result.Fail("The Request Has Not Been Done Completely, Try Again");

            return Result.Fail("Invalid, More Than 10 minutes has passed since the transaction time, try another transaction");
        }

        // if 'transactionStatus' from db is "Pending"
        Transaction? updatedTransaction = null;
        if (confirmTransactionRequest.TransactionStatus == nameof(TransactionStatusOptions.Confirmed))
        {
            updatedTransaction = _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Confirmed);
            var validateResult = await ApplyTransactionFee(transaction, fromAccount!, toAccount!);
            // if (validateResult.IsFailed) return validateResult.ToResult();
            if (validateResult.IsFailed) return Result.Fail(validateResult.FirstErrorMessage());
        }
        else if (confirmTransactionRequest.TransactionStatus == nameof(TransactionStatusOptions.Cancelled))
        {
            updatedTransaction = _transactionRepository.UpdateTransactionStatusOfTransaction(transaction, TransactionStatusOptions.Cancelled);
            DiscardTransactionFee(transaction, fromAccount!, toAccount!);
        }
        
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if ((transaction.TransactionType == TransactionTypeOptions.Deposit && !(numberOfRowsAffected >= 2)) ||
            (transaction.TransactionType == TransactionTypeOptions.Transfer && !(numberOfRowsAffected >= 3))) 
            return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok(updatedTransaction?.ToTransactionResponse()!);      
    }
    
    public async Task<Result<List<TransactionResponse>>> GetAllTransactions(ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' object parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail(CreateNotFoundError("The User Doesn't Exist")); // if 'user' doesn't exist

        if (await _userManager.IsInRoleAsync(user, Constants.AdminRole))
            return await GetAllTransactionsInternal();
        
        var transactions = await _transactionRepository.GetAllTransactionsByUserAsync(user!.Id);
        
        var transactionResponses = transactions.Select(transactionItem => transactionItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }
    
    public async Task<List<TransactionResponse>> GetAllTransactionsInternal()
    {
        var transactions = await _transactionRepository.GetAllTransactionsAsync(ignoreQueryFilter:true);
        var transactionResponses = transactions.Select(transactionItem => transactionItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }

    public async Task<Result<TransactionResponse>> GetTransactionByID(Guid id, ClaimsPrincipal userClaims, bool ignoreQueryFilter = false)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' parameter is Null");

        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail("The User Doesn't Exist"); // if 'user' doesn't exist

        if (await _userManager.IsInRoleAsync(user, Constants.AdminRole))
            return await GetTransactionByIDInternal(id);

        var transaction = await _transactionRepository.GetTransactionByIDAsync(id, ignoreQueryFilter);
        if (transaction == null) return Result.Fail(CreateNotFoundError("!!A Transaction With This ID Has Not Been Found!!")); // if 'ID' is invalid (doesn't exist)

        if (transaction.FromAccount!.OwnerID != user!.Id && transaction.ToAccount!.OwnerID != user!.Id)
            return Result.Fail("This Transaction Doesn't Belong To You");
        
        return Result.Ok(transaction.ToTransactionResponse());
    }

    public async Task<Result<TransactionResponse>> GetTransactionByIDInternal(Guid id)
    {
        var transaction = await _transactionRepository.GetTransactionByIDAsync(id, ignoreQueryFilter:true);
        if (transaction == null) return Result.Fail(CreateNotFoundError("!!A Transaction With This ID Has Not Been Found!!")); // if 'ID' is invalid (doesn't exist)

        return Result.Ok(transaction.ToTransactionResponse());
    }  
    
    public async Task<Result> DeleteTransactionById(Guid id, ClaimsPrincipal userClaims)
    {
        ArgumentNullException.ThrowIfNull(userClaims,$"The '{nameof(userClaims)}' parameter is Null");
        
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return Result.Fail("The User Doesn't Exist"); // if 'user' doesn't exist

        if (await _userManager.IsInRoleAsync(user, Constants.AdminRole))
            return await DeleteTransactionByIdInternal(id);
        
        var transaction = await _transactionRepository.GetTransactionByIDAsync(id);
        if (transaction == null) return Result.Fail(CreateNotFoundError("!!A Transaction With This ID Has Not Been Found!!")); // if 'ID' is invalid (doesn't exist)
        
        if (transaction.FromAccount!.OwnerID != user.Id)
            return Result.Fail("This Transaction Doesn't Belong To You");
    
        _transactionRepository.DeleteTransaction(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Not Be b en Successful");
    }
    
    public async Task<Result> DeleteTransactionByIdInternal(Guid id)
    {
        var transaction = await _transactionRepository.GetTransactionByIDAsync(id, ignoreQueryFilter:true);
        if (transaction == null) return Result.Fail(CreateNotFoundError("!!A Transaction With This ID Has Not Been Found!!")); // if 'ID' is invalid (doesn't exist)
        
        _transactionRepository.DeleteTransaction(transaction);
        var numberOfRowsAffected = await _transactionRepository.SaveChangesAsync();
        if (!(numberOfRowsAffected > 0)) return Result.Fail("The Request Has Not Been Done Completely, Try Again");

        return Result.Ok().WithSuccess("The Deletion Has Not Be b en Successful");
    }
    
    
    
    
    
    
    
    // Private Methods //
    
    private Result CheckAccountAccess(TransactionTransferAddRequest transactionAddRequest, UserProfile user)
    {
        ArgumentNullException.ThrowIfNull(transactionAddRequest,$"The '{nameof(transactionAddRequest)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(user,$"The '{nameof(user)}' object parameter is Null");

        // if 'FromAccountNumber' and 'ToAccountNumber' be Same
        if (transactionAddRequest.FromAccountNumber == transactionAddRequest.ToAccountNumber)
            return Result.Fail("'FromAccountNumber' and 'ToAccountNumber' can't be Same");
        
        // 'FromAccountNumber' has to belong to the user itself //
        if (!user.CurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.FromAccountNumber))
            return Result.Fail("'FromAccountNumber' is Not One of Your Accounts Number");
        
        // 'ToAccountNumber' has to be one of the user DefinedAccounts Number //
        if (!user.DefinedCurrencyAccounts!.Any(acc => acc.Number == transactionAddRequest.ToAccountNumber))
            return Result.Fail("'ToAccountNumber' is Not One of Your DefinedAccounts Number");

        return Result.Ok();
    }
    
    private async Task<Result<TransferCalculationsAmounts>> CalculateTransferAmounts(CurrencyAccount fromAccount, CurrencyAccount toAccount,
                                                                                                                   Transaction transaction, bool isCommissionFree)
    {
        ArgumentNullException.ThrowIfNull(fromAccount,$"The '{nameof(fromAccount)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(toAccount,$"The '{nameof(toAccount)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(transaction,$"The '{nameof(transaction)}' object parameter is Null");

        
        // Calculate Commission to be subtracted from 'StashBalance' of 'FromAccount'
        decimal commissionAmount = 0;
        decimal? cRate = null;
        if (!isCommissionFree)
        {
            var money = new Money() { Amount = transaction.Amount, Currency = fromAccount.Currency!};
            var cRateValidateResult = await _commissionRateService.GetUSDAmountCRate(money);
            // if (cRateValidateResult.IsFailed) return cRateValidateResult.ToResult();
            if (cRateValidateResult.IsFailed) return Result.Fail(cRateValidateResult.FirstErrorMessage());

            cRate = cRateValidateResult.Value;
            commissionAmount = transaction.Amount * cRate.GetValueOrDefault();
        }
        
        // Calculate Final Amount to be subtracted from 'StashBalance' of 'FromAccount'
        var decreaseAmount = transaction.Amount + commissionAmount;
        var currencyType = fromAccount.Currency?.CurrencyType;

        var balanceValidateResult = await CheckFromAccountInvalidBalance(fromAccount, currencyType!, decreaseAmount, transaction.DateTime);
        // if (balanceValidateResult.IsFailed) return balanceValidateResult.ToResult();
        if (balanceValidateResult.IsFailed) return Result.Fail(balanceValidateResult.FirstErrorMessage());
        
        // Calculate Final Amount to be added to 'StashBalance' of 'ToAccount'
        var (exchangeValidateResult, valueToBeMultiplied) = (await _exchangeValueService.GetExchangeValueByCurrencyTypes(fromAccount.Currency?.CurrencyType!, toAccount.Currency?.CurrencyType!)).DeconstructObject();
        if (exchangeValidateResult.IsFailed) return Result.Fail($"There is No Relevant Exchange Value to convert to {toAccount.Currency?.CurrencyType!}");
        var destinationAmount = transaction.Amount * valueToBeMultiplied;
        
        var allTransactionAmounts = new TransferCalculationsAmounts()
        {
            CRate = cRate!.Value,
            CommissionAmount = commissionAmount,
            DestinationAmount = destinationAmount,
            ValueToBeMultiplied = valueToBeMultiplied
        };
        return Result.Ok(allTransactionAmounts);
    }

    private async Task<Result> CheckFromAccountInvalidBalance(CurrencyAccount fromAccount, string currencyType, decimal decreaseAmount, DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(fromAccount,$"The '{nameof(fromAccount)}' object parameter is Null");
        
        var finalBalanceAmount = fromAccount.Balance - decreaseAmount;

        var minimumUSDValidateResult = await CheckMinimumUSDBalanceAsync(currencyType, finalBalanceAmount);
        // if (minimumUSDValidateResult.IsFailed) return minimumUSDValidateResult.ToResult(); 
        if (minimumUSDValidateResult.IsFailed) return Result.Fail(minimumUSDValidateResult.FirstErrorMessage()); 
        
        var maxUSDDayValidateResult = await CheckMaxUSDBalanceWithdrawPerDayAsync(currencyType, decreaseAmount, fromAccount, dateTime);
        // if (maxUSDDayValidateResult.IsFailed) return maxUSDDayValidateResult.ToResult(); 
        if (maxUSDDayValidateResult.IsFailed) return Result.Fail(maxUSDDayValidateResult.FirstErrorMessage()); 
        
        return Result.Ok();
    }
    
    private async Task<Result> CheckMinimumUSDBalanceAsync(string currencyType, decimal finalAmount)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' object parameter is Null");
        
        var (exchangeValidateResult, valueToBeMultiplied) = (await _exchangeValueService.GetExchangeValueByCurrencyTypes(currencyType, Constants.USDCurrency)).DeconstructObject();
        if (exchangeValidateResult.IsFailed) return Result.Fail($"There is No Relevant Exchange Value to convert to {Constants.USDCurrency}");
        
        var finalUSDAmount = finalAmount * valueToBeMultiplied;
        if (finalUSDAmount < BalanceMinimumUSD)
            return Result.Fail("The Balance Amount will be under 50 USD Dollars, This is Invalid");
        
        return Result.Ok(); 
    }
    
    private async Task<Result> CheckMaxUSDBalanceWithdrawPerDayAsync(string currencyType, decimal decreaseAmount, CurrencyAccount currencyAccount, DateTime transactionDate)
    {
        ArgumentNullException.ThrowIfNull(currencyType,$"The '{nameof(currencyType)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(currencyAccount,$"The '{nameof(currencyAccount)}' object parameter is Null");

        var (exchangeValidateResult, valueToBeMultiplied) = (await _exchangeValueService.GetExchangeValueByCurrencyTypes(currencyType, Constants.USDCurrency)).DeconstructObject();
        if (exchangeValidateResult.IsFailed) return Result.Fail($"There is No Relevant Exchange Value to convert to {Constants.USDCurrency}");

        var userAllTransactions = await _transactionRepository.GetAllTransactionsByUserAsync(currencyAccount.OwnerID);
        var userAllFromTransactionsPerDay = userAllTransactions.Where(t => t.FromAccountNumber == currencyAccount.Number &&
                                                                           t.TransactionType == TransactionTypeOptions.Transfer &&
                                                                           t.TransactionStatus == TransactionStatusOptions.Confirmed)
                                                               .GroupBy(t => t.DateTime.Date).ToList();
        var userAllFromTransactionsToday = userAllFromTransactionsPerDay.FirstOrDefault(group => group.Key == transactionDate.Date);
        if (userAllFromTransactionsToday == null) return Result.Ok();
        var finalAmount = userAllFromTransactionsToday.Sum(t => t.FromAccountChangeAmount);
        
        var finalUSDAmount = finalAmount * valueToBeMultiplied;
        if ((finalUSDAmount + decreaseAmount) > BalanceMaxWithdrawPerDay)
            return Result.Fail("With This Transaction,The Balance Amount Deposit for Today is more than 1000 USD Dollars, This is Invalid");
        
        return Result.Ok();
    }
    
    
    private Result CheckInvalidTransactionStatus(TransactionStatusOptions transactionStatus) => transactionStatus switch
    {
        TransactionStatusOptions.Confirmed => Result.Fail("Transaction is Already Confirmed"),
        TransactionStatusOptions.Cancelled => Result.Fail("Transaction is Already Cancelled"),
        TransactionStatusOptions.Failed => Result.Fail("Transaction is Failed Before and Can't Be Confirmed/Cancelled"),
        _ => Result.Ok()
    };
    
    private async Task<Result> ApplyTransactionFee(Transaction transaction, CurrencyAccount fromAccount, CurrencyAccount? toAccount)
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
            var decreaseAmount = transaction.FromAccountChangeAmount;
            
            var balanceValidateResult = await CheckFromAccountInvalidBalance(fromAccount, fromAccount.Currency!.CurrencyType, decreaseAmount, transaction.DateTime);
            // if (balanceValidateResult.IsFailed) return balanceValidateResult.ToResult();
            if (balanceValidateResult.IsFailed) return Result.Fail(balanceValidateResult.FirstErrorMessage());
            
            _currencyAccountService.UpdateStashBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 + val2);
            _currencyAccountService.UpdateBalanceAmount(fromAccount, transaction.FromAccountChangeAmount, (val1, val2) => val1 - val2);

            _currencyAccountService.UpdateStashBalanceAmount(toAccount!, transaction.ToAccountChangeAmount, (val1, val2) => val1 - val2);
            _currencyAccountService.UpdateBalanceAmount(toAccount!, transaction.ToAccountChangeAmount, (val1, val2) => val1 + val2);
        }

        return Result.Ok();
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