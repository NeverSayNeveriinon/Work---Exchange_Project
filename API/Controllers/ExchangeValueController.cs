using Core.DTO.ExchangeValueDTO;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[Route("api/[controller]")] 
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class ExchangeValueController : ControllerBase
{
    private readonly IExchangeValueService _exchangeValueService;
    
    public ExchangeValueController(IExchangeValueService exchangeValueService)
    {
        _exchangeValueService = exchangeValueService;
    }
    
    [Route("index")]
    [ApiExplorerSettings(IgnoreApi = true)] // For not showing in 'Swagger'
    public IActionResult Index()
    {
        return Content("Here is the \"ExchangeValue\" Home Page");
    }
    
    
    
    /// <summary>
    /// Get All Existing ExchangeValues
    /// </summary>
    /// <returns>The ExchangeValues List</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/ExchangeValue"
    /// 
    /// </remarks>
    /// <response code="200">The ExchangeValues List is successfully returned</response>
    [HttpGet]
    // GET: api/ExchangeValue
    public async Task<ActionResult<IEnumerable<ExchangeValueResponse>>> GetExchangeValues()
    {
        List<ExchangeValueResponse> exchangeValuesList = await _exchangeValueService.GetAllExchangeValues();
        return Ok(exchangeValuesList);
    }
    
     
    /// <summary>
    /// Add a New ExchangeValue to ExchangeValues List
    /// </summary>
    /// <returns>Redirect to 'GetExchangeValue' action to return ExchangeValue That Has Been Added</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     POST -> "api/ExchangeValue"
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The New ExchangeValue is successfully added to ExchangeValues List</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [HttpPost]
    // Post: api/ExchangeValue
    public async Task<IActionResult> PostExchangeValue(ExchangeValueAddRequest exchangeValueAddRequest)
    {
        var exchangeValueResponse = await _exchangeValueService.AddExchangeValue(exchangeValueAddRequest);
        
        return CreatedAtAction(nameof(GetExchangeValue), new {exchangeValueID = exchangeValueResponse.Id}, new { exchangeValueResponse.Id });
    }

    
    
    /// <summary>
    /// Get an Existing ExchangeValue Based On Given ID
    /// </summary>
    /// <returns>The ExchangeValue That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/ExchangeValue/..."
    /// 
    /// </remarks>
    /// <response code="200">The ExchangeValue is successfully found and returned</response>
    /// <response code="404">A ExchangeValue with Given ID has not been found</response>
    [HttpGet("{exchangeValueID:int}")]
    // GET: api/ExchangeValue/{exchangeValueID}
    public async Task<ActionResult<ExchangeValueResponse>> GetExchangeValue(int exchangeValueID)
    {
        ExchangeValueResponse? exchangeValueObject = await _exchangeValueService.GetExchangeValueByID(exchangeValueID);
        if (exchangeValueObject is null)
        {
            return NotFound("notfound:");
        }
        
        return Ok(exchangeValueObject);
    }
    
    
    /// <summary>
    /// Update an Existing ExchangeValue Based on Given ID and New ExchangeValue Object
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Put -> "api/ExchangeValue/..."
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">The ExchangeValue is successfully found and has been updated with New ExchangeValue</response>
    /// <response code="404">A ExchangeValue with Given ID has not been found</response>
    // /// <response code="400">The ID in Url doesn't match with the ID in Body</response>
    [HttpPut("{exchangeValueID:int}")]
    // Put: api/ExchangeValue/{exchangeValueID}
    public async Task<IActionResult> PutExchangeValue(ExchangeValueUpdateRequest exchangeValueUpdateRequest, int exchangeValueID)
    {
        ExchangeValueResponse? existingObject = await _exchangeValueService.UpdateExchangeValue(exchangeValueUpdateRequest, exchangeValueID);
        if (existingObject is null)
        {
            return NotFound("notfound:");
        }
        
        return NoContent();
    }
    
    
    /// <summary>
    /// Delete an Existing ExchangeValue Based on Given ID
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/ExchangeValue/..."
    /// 
    /// </remarks>
    /// <response code="204">The ExchangeValue is successfully found and has been deleted from ExchangeValues List</response>
    /// <response code="404">A ExchangeValue with Given ID has not been found</response>
    [HttpDelete("{exchangeValueID:int}")]
    // Delete: api/ExchangeValue/{exchangeValueID}
    public async Task<IActionResult> DeleteExchangeValue(int exchangeValueID)
    {
        bool? exchangeValueObject = await _exchangeValueService.DeleteExchangeValue(exchangeValueID);
        if (exchangeValueObject is null)
        {
            return NotFound("notfound:");
        }

        return NoContent();
    }
}