using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyAccountDTO;

namespace Core.Services;

public class CurrencyAccountService
{
    private readonly ICurrencyAccountRepository _accountRepository;
    
    public CurrencyAccountService(ICurrencyAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountRequest? currencyAccountRequest)
    {
        // 'currencyAccountRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyAccountRequest,"The 'CurrencyAccountRequest' object parameter is Null");
        
        // ValidationHelper.ModelValidation(currencyAccountRequest);

        // // 'currencyAccountRequest.Name' is Duplicate //
        // // Way 1
        // if ( (await _accountRepository.GetFilteredCurrencyAccounts(currencyAccount => currencyAccount.Name == currencyAccountRequest.Name))?.Count > 0)
        // {
        //     throw new ArgumentException("The 'CurrencyAccount Name' is already exists");
        // }
        
        
        // 'currencyAccountRequest.Name' is valid and there is no problem //
        CurrencyAccount currencyAccount = currencyAccountRequest.ToCurrencyAccount();
        CurrencyAccount currencyAccountReturned = await _accountRepository.AddCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return currencyAccountReturned.ToCurrencyAccountResponse();
    }   

    public async Task<List<CurrencyAccountResponse>> GetAllCurrencyAccounts()
    {
        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        List<CurrencyAccount> currencyAccounts = await _accountRepository.GetAllCurrencyAccounts();
        
        List<CurrencyAccountResponse> currencyAccountResponses = currencyAccounts.Select(accountItem => accountItem.ToCurrencyAccountResponse()).ToList();
        return currencyAccountResponses;
    }

    public async Task<CurrencyAccountResponse?> GetCurrencyAccountByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CurrencyAccount'Id' parameter is Null");

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByID(Id.Value);

        // if 'id' doesn't exist in 'currencyAccounts list' 
        if (currencyAccount == null)
        {
            return null;
        }

        // if there is no problem
        CurrencyAccountResponse currencyAccountResponse = currencyAccount.ToCurrencyAccountResponse();

        return currencyAccountResponse;;
    }

    public async Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountRequest? currencyAccountRequest, int? currencyAccountID)
    {
        // if 'currencyAccount ID' is null
        ArgumentNullException.ThrowIfNull(currencyAccountID,"The CurrencyAccount'ID' parameter is Null");
        
        // if 'currencyAccountRequest' is null
        ArgumentNullException.ThrowIfNull(currencyAccountRequest,"The 'CurrencyAccountRequest' object parameter is Null");

        
        // Validation
        // ValidationHelper.ModelValidation(currencyAccountRequest);

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByID(currencyAccountID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currencyAccount == null)
        {
            return null;
        }
            
        CurrencyAccount updatedCurrencyAccount = await _accountRepository.UpdateCurrencyAccount(currencyAccount, currencyAccountRequest.ToCurrencyAccount());
        await _accountRepository.SaveChangesAsync();

        return updatedCurrencyAccount.ToCurrencyAccountResponse();
    }

    public async Task<bool?> DeleteCurrencyAccount(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The CurrencyAccount'ID' parameter is Null");

        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByID(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currencyAccount == null) 
        {
            return null;
        }
    
        bool result = await _accountRepository.DeleteCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return result;
    }
}