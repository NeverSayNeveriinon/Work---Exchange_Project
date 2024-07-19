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

    private static readonly decimal BalanceMinimumUSD = 50M;
    
    
    public async Task<TransactionResponse?> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest, ClaimsPrincipal userClaims)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        var user = await _userManager.GetUserAsync(userClaims);
        var userAccountsId = user.CurrencyAccounts.Select(account => account.Number).ToHashSet();
        if (!userAccountsId.Contains(transactionAddRequest.FromAccountNumber))
        {
            return null;
        }
        
        var userDefinedAccountsId = user?.DefinedAccountNumbers?.ToHashSet();
        if (!userDefinedAccountsId.Contains(transactionAddRequest.ToAccountNumber))
        {
            return null;
        }

        bool isCommissionFree = false;
        if (!userAccountsId.Contains(transactionAddRequest.ToAccountNumber))
        {
            isCommissionFree = true;
        }
        
        Transaction transaction = transactionAddRequest.ToTransaction();
        _transactionRepository.LoadReferences(transaction);
           
        
        //
        var sourceAmount = transactionAddRequest.Amount;
        
        // Commission
        decimal amountCommission = 0;
        if (!isCommissionFree)
        {
            var money = new Money()
            {
                Amount = sourceAmount.Value,
                Currency = transaction.FromAccount!.Currency!
            };
            var cRate = await _commissionRateService.GetCRate(money);
            amountCommission = sourceAmount.Value * cRate;
        }
        
        // 
        var valueToBeMultiplied = transaction?.FromAccount?.Currency?.FirstExchangeValues?
            .FirstOrDefault(exValue => exValue.FirstCurrencyId == transaction.ToAccount?.CurrencyID)
            ?.UnitOfFirstValue;
        var destinationAmount = sourceAmount * valueToBeMultiplied.Value;
        
        // for FromAccount
        var decreaseAmount = sourceAmount + amountCommission;
        var finalAmount = transaction.FromAccount.Balance - decreaseAmount;
        var currencyType = transaction.FromAccount.Currency == null ? null : Enum.GetName(typeof(CurrencyTypeOptions), transaction.FromAccount.Currency.CurrencyType);
        var isValid = await CheckMinimumUSDBalanceAsync(currencyType, finalAmount.Value);
        if (isValid == false)
        {
            return null;
        }
        
        await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, sourceAmount, (val1, val2) => val1 - val2);
        if (!isCommissionFree) await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, amountCommission, (val1, val2) => val1 - val2);

        
        // for ToAccount
        await _currencyAccountService.UpdateBalanceAmount(transaction.ToAccount, destinationAmount, (val1, val2) => val1 + val2);

        
        Transaction transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();
        
        return transactionAddReturned.ToTransactionResponse();
    }

    public async Task<TransactionResponse?> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        Transaction transaction = transactionAddRequest.ToTransaction();
        _transactionRepository.LoadReferences(transaction);

        
        var isValid = await CheckMinimumUSDBalanceAsync(transactionAddRequest.Money.CurrencyType, transactionAddRequest.Money.Amount.Value);
        if (isValid == false)
        {
            return null;
        }
        
        var moneyCurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), transactionAddRequest.Money.CurrencyType);
        var valueToBeMultiplied = transaction?.FromAccount?.Currency?.SecondExchangeValues?.FirstOrDefault(exValue=> exValue.FirstCurrency.CurrencyType == moneyCurrencyType)!.UnitOfFirstValue;
        var amount = transactionAddRequest.Money.Amount * valueToBeMultiplied.Value;
        
        //
        await _currencyAccountService.UpdateBalanceAmount(new CurrencyAccount(), amount, (val1, val2) => val1 + val2);
        
        
        Transaction transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // var transactionGetReturned = await _transactionRepository.GetTransactionByIDAsync(transactionAddReturned.Id);
        
        return transactionAddReturned.ToTransactionResponse();
    }

    private async Task<bool> CheckMinimumUSDBalanceAsync(string currencyType, decimal finalAmount)
    {
        var valueToBeMultiplied = await _exchangeValueService.GetEquivalentUSDByCurrencyType(currencyType);
        var finalUSDAmount = finalAmount * valueToBeMultiplied;
        if (finalUSDAmount < BalanceMinimumUSD)
        {
            return false;
        }
        return true;
    }

    
    public async Task<TransactionResponse?> UpdateIsConfirmedOfTransaction(int? transactionId, bool? isConfirmed, TimeSpan TimeNow)
    {
        // if 'transactionId' is null
        ArgumentNullException.ThrowIfNull(transactionId,"The 'transactionId' parameter is Null");
        
        // if 'isConfirmed' is null
        ArgumentNullException.ThrowIfNull(isConfirmed,"The 'isConfirmed' parameter is Null");
        
        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(transactionId.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null)
        {
            return null;
        }
    
        // if More Than 10 minutes has passed since the transaction time
        if (transaction.DateTime.TimeOfDay.Minutes - TimeNow.Minutes > 10)
        {
            return null;
        }
            
        Transaction updatedTransaction = _transactionRepository.UpdateIsConfirmedOfTransaction(transaction, isConfirmed.Value);
        await _transactionRepository.SaveChangesAsync();

        return updatedTransaction.ToTransactionResponse();
    }
    
    
    public async Task<List<TransactionResponse>> GetAllTransactions()
    {
        List<Transaction> transactions = await _transactionRepository.GetAllTransactionsAsync();
        
        List<TransactionResponse> transactionResponses = transactions.Select(accountItem => accountItem.ToTransactionResponse()).ToList();
        return transactionResponses;
    }

    public async Task<TransactionResponse?> GetTransactionByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'Id' parameter is Null");

        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);

        // if 'id' doesn't exist in 'transactions list' 
        if (transaction == null)
        {
            return null;
        }

        // if there is no problem
        TransactionResponse transactionResponse = transaction.ToTransactionResponse();

        return transactionResponse;
    }

    public async Task<TransactionResponse?> UpdateTransaction(TransactionUpdateRequest? transactionUpdateRequest, int? transactionID)
    {
        // if 'transaction ID' is null
        ArgumentNullException.ThrowIfNull(transactionID,"The Transaction'ID' parameter is Null");
        
        // if 'transactionUpdateRequest' is null
        ArgumentNullException.ThrowIfNull(transactionUpdateRequest,"The 'TransactionUpdateRequest' object parameter is Null");
        
        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(transactionID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null)
        {
            return null;
        }
            
        Transaction updatedTransaction = _transactionRepository.UpdateTransaction(transaction, transactionUpdateRequest.ToTransaction());
        await _transactionRepository.SaveChangesAsync();

        return updatedTransaction.ToTransactionResponse();
    }

    public async Task<bool?> DeleteTransaction(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The Transaction'ID' parameter is Null");

        Transaction? transaction = await _transactionRepository.GetTransactionByIDAsync(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (transaction == null) 
        {
            return null;
        }
    
        bool result = _transactionRepository.DeleteTransaction(transaction);
        await _transactionRepository.SaveChangesAsync();

        return result;
    }
}