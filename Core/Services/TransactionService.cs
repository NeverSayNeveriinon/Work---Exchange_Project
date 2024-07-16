using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.TransactionDTO;
using Core.Enums;
using Core.ServiceContracts;

namespace Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICommissionRateService _commissionRateService;
    private readonly ICurrencyAccountService _currencyAccountService;
    
    public TransactionService(ITransactionRepository transactionRepository, ICommissionRateService commissionRateService, ICurrencyAccountService currencyAccountService)
    {
        _transactionRepository = transactionRepository;
        _commissionRateService = commissionRateService;
        _currencyAccountService = currencyAccountService;
    }

    public async Task<TransactionResponse> AddTransferTransaction(TransactionTransferAddRequest? transactionAddRequest)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        Transaction transaction = transactionAddRequest.ToTransaction();
        _transactionRepository.LoadReferences(transaction);
        
        
        //
        var sourceAmount = transactionAddRequest.Amount;
        await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, sourceAmount, (val1, val2) => val1 - val2);
        
        //
        var valueToBeMultiplied = transaction?.FromAccount?.Currency?.FirstExchangeValues?
            .FirstOrDefault(exValue => exValue.FirstCurrencyId == transaction.ToAccount?.CurrencyID)
            ?.UnitOfFirstValue;
        var destinationAmount = sourceAmount * valueToBeMultiplied.Value;
        await _currencyAccountService.UpdateBalanceAmount(transaction.ToAccount, destinationAmount, (val1, val2) => val1 + val2);
    
        //
        var money = new Money()
        {
            Amount = sourceAmount,
            Currency = transaction.FromAccount!.Currency!
        };
        var cRate = await _commissionRateService.GetCRate(money);
        var amountCommission = sourceAmount * cRate;
        await _currencyAccountService.UpdateBalanceAmount(transaction.FromAccount, amountCommission, (val1, val2) => val1 - val2);

        
        Transaction transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // var transactionGetReturned = await _transactionRepository.GetTransactionByIDAsync(transactionAddReturned.Id);
        
        return transactionAddReturned.ToTransactionResponse();
    }

    public async Task<TransactionResponse> AddDepositTransaction(TransactionDepositAddRequest? transactionAddRequest)
    {
        // 'transactionAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(transactionAddRequest,"The 'transactionAddRequest' object parameter is Null");
        
        Transaction transaction = transactionAddRequest.ToTransaction();
        _transactionRepository.LoadReferences(transaction);

        
        var moneyCurrencyType = (CurrencyTypeOptions)Enum.Parse(typeof(CurrencyTypeOptions), transactionAddRequest.money.CurrencyType);
        var valueToBeMultiplied = transaction?.FromAccount?.Currency?.SecondExchangeValues?.FirstOrDefault(exValue=> exValue.FirstCurrency.CurrencyType == moneyCurrencyType)!.UnitOfFirstValue;
        var amount = transactionAddRequest.money.Amount * valueToBeMultiplied.Value;
        
        
        //
        await _currencyAccountService.UpdateBalanceAmount(new CurrencyAccount(), amount, (val1, val2) => val1 + val2);
        
        
        Transaction transactionAddReturned = await _transactionRepository.AddTransactionAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // var transactionGetReturned = await _transactionRepository.GetTransactionByIDAsync(transactionAddReturned.Id);
        
        return transactionAddReturned.ToTransactionResponse();
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