using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.CurrencyAccountDTO;
using Core.ServiceContracts;

namespace Core.Services;

public class CurrencyAccountService : ICurrencyAccountService
{
    private readonly ICurrencyAccountRepository _accountRepository;
    
    public CurrencyAccountService(ICurrencyAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<CurrencyAccountResponse> AddCurrencyAccount(CurrencyAccountAddRequest? currencyAccountAddRequest)
    {
        // 'CurrencyAccountAddRequest' is Null //
        ArgumentNullException.ThrowIfNull(currencyAccountAddRequest,"The 'CurrencyAccountAddRequest' object parameter is Null");
        
        // ValidationHelper.ModelValidation(currencyAccountAddRequest);

        // // 'CurrencyAccountAddRequest.Name' is Duplicate //
        // // Way 1
        // if ( (await _accountRepository.GetFilteredCurrencyAccounts(currencyAccount => currencyAccount.Name == CurrencyAccountAddRequest.Name))?.Count > 0)
        // {
        //     throw new ArgumentException("The 'CurrencyAccount Name' is already exists");
        // }
        
        
        // 'CurrencyAccountAddRequest.Name' is valid and there is no problem //
        CurrencyAccount currencyAccount = currencyAccountAddRequest.ToCurrencyAccount();
        CurrencyAccount currencyAccountReturned = await _accountRepository.AddCurrencyAccount(currencyAccount);
        await _accountRepository.SaveChangesAsync();

        return currencyAccountReturned.ToCurrencyAccountResponse();
    }   
    
    // public async Task<CurrencyAccountResponse> AddDefinedAccount(int? currencyDefinedAccountAdd)
    // {
    //     // 'currencyDefinedAccountAdd' is Null //
    //     ArgumentNullException.ThrowIfNull(currencyDefinedAccountAdd,"The 'currencyDefinedAccountAdd' object parameter is Null");
    //     
    //     // 'currencyDefinedAccountAdd.Name' is valid and there is no problem //
    //     CurrencyAccount currencyAccount = currencyAccountAddRequest.ToCurrencyAccount();
    //     CurrencyAccount currencyAccountReturned = await _accountRepository.AddCurrencyAccount(currencyAccount);
    //     await _accountRepository.SaveChangesAsync();
    //
    //     return currencyAccountReturned.ToCurrencyAccountResponse();
    // }   
    
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

    public async Task<CurrencyAccountResponse?> UpdateCurrencyAccount(CurrencyAccountUpdateRequest? currencyAccountUpdateRequest, int? currencyAccountID)
    {
        // if 'currencyAccount ID' is null
        ArgumentNullException.ThrowIfNull(currencyAccountID,"The CurrencyAccount'ID' parameter is Null");
        
        // if 'CurrencyAccountUpdateRequest' is null
        ArgumentNullException.ThrowIfNull(currencyAccountUpdateRequest,"The 'CurrencyAccountUpdateRequest' object parameter is Null");

        
        // Validation
        // ValidationHelper.ModelValidation(CurrencyAccountUpdateRequest);

        // const string includeEntities = "Director,Writers,Artists,Genres"; 
        CurrencyAccount? currencyAccount = await _accountRepository.GetCurrencyAccountByID(currencyAccountID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (currencyAccount == null)
        {
            return null;
        }
            
        CurrencyAccount updatedCurrencyAccount = await _accountRepository.UpdateCurrencyAccount(currencyAccount, currencyAccountUpdateRequest.ToCurrencyAccount());
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