using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.ExchangeValueDTO;
using Core.ServiceContracts;

namespace Core.Services;


public class ExchangeValueService : IExchangeValueService
{
    private readonly IExchangeValueRepository _exchangeValueRepository;
    private IExchangeValueService _exchangeValueServiceImplementation;

    public ExchangeValueService(IExchangeValueRepository exchangeValueRepository)
    {
        _exchangeValueRepository = exchangeValueRepository;
    }

    public async Task<ExchangeValueResponse> AddExchangeValue(ExchangeValueAddRequest? exchangeValueAddRequest)
    {
        // 'exchangeValueRequest' is Null //
        ArgumentNullException.ThrowIfNull(exchangeValueAddRequest,"The 'ExchangeValueRequest' object parameter is Null");
        
        // 'exchangeValueRequest.Name' is valid and there is no problem //
        ExchangeValue exchangeValue = exchangeValueAddRequest.ToExchangeValue();
        ExchangeValue exchangeValueReturned = await _exchangeValueRepository.AddExchangeValue(exchangeValue);
        await _exchangeValueRepository.SaveChangesAsync();

        return exchangeValueReturned.ToExchangeValueResponse();
    }   
    

    public async Task<List<ExchangeValueResponse>> GetAllExchangeValues()
    {
        List<ExchangeValue> exchangeValues = await _exchangeValueRepository.GetAllExchangeValues();
        
        List<ExchangeValueResponse> exchangeValueResponses = exchangeValues.Select(accountItem => accountItem.ToExchangeValueResponse()).ToList();
        return exchangeValueResponses;
    }

    public async Task<ExchangeValueResponse?> GetExchangeValueByID(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'Id' parameter is Null");

        ExchangeValue? exchangeValue = await _exchangeValueRepository.GetExchangeValueByID(Id.Value);

        // if 'id' doesn't exist in 'exchangeValues list' 
        if (exchangeValue == null)
        {
            return null;
        }

        // if there is no problem
        ExchangeValueResponse exchangeValueResponse = exchangeValue.ToExchangeValueResponse();

        return exchangeValueResponse;;
    }

    public async Task<ExchangeValueResponse?> UpdateExchangeValue(ExchangeValueUpdateRequest? exchangeValueUpdateRequest, int? exchangeValueID)
    {
        // if 'exchangeValue ID' is null
        ArgumentNullException.ThrowIfNull(exchangeValueID,"The ExchangeValue'ID' parameter is Null");
        
        // if 'exchangeValueRequest' is null
        ArgumentNullException.ThrowIfNull(exchangeValueUpdateRequest,"The 'ExchangeValueRequest' object parameter is Null");
        

        ExchangeValue? exchangeValue = await _exchangeValueRepository.GetExchangeValueByID(exchangeValueID.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null)
        {
            return null;
        }
            
        ExchangeValue updatedExchangeValue = await _exchangeValueRepository.UpdateExchangeValue(exchangeValue, exchangeValueUpdateRequest.ToExchangeValue());
        await _exchangeValueRepository.SaveChangesAsync();

        return updatedExchangeValue.ToExchangeValueResponse();
    }

    public async Task<bool?> DeleteExchangeValue(int? Id)
    {
        // if 'id' is null
        ArgumentNullException.ThrowIfNull(Id,"The ExchangeValue'ID' parameter is Null");

        ExchangeValue? exchangeValue = await _exchangeValueRepository.GetExchangeValueByID(Id.Value);
        
        // if 'ID' is invalid (doesn't exist)
        if (exchangeValue == null) 
        {
            return null;
        }
    
        bool result = await _exchangeValueRepository.DeleteExchangeValue(exchangeValue);
        await _exchangeValueRepository.SaveChangesAsync();

        return result;
    }
}