using Core.DTO.ExchangeValueDTO;
using Core.Helpers;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[Route("api/[controller]")] 
[ApiController]
[Authorize(Roles = Constants.Role.Admin)]
public class ExchangeValueController : ControllerBase
{
    private readonly IExchangeValueService _exchangeValueService;
    private readonly ICurrencyValidator _validator ;
    
    public ExchangeValueController(IExchangeValueService exchangeValueService, ICurrencyValidator validator)
    {
        _exchangeValueService = exchangeValueService;
        _validator = validator;
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
    public async Task<ActionResult<IEnumerable<ExchangeValueResponse>>> GetAllExchangeValues()
    {
        var exchangeValuesList = await _exchangeValueService.GetAllExchangeValues();
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
    public async Task<IActionResult> AddExchangeValue(ExchangeValueAddRequest exchangeValueAddRequest)
    {
        bool isValid = await _validator.ExistsInCurrentCurrencies(exchangeValueAddRequest.FirstCurrencyType) &&
                       await _validator.ExistsInCurrentCurrencies(exchangeValueAddRequest.SecondCurrencyType);
        if (!isValid)
        {
            ModelState.AddModelError("CurrencyType", "The CurrencyType is not in Current Currencies");
            return new BadRequestObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext,ModelState));
        }
        
        var res = await _exchangeValueService.AddExchangeValue(exchangeValueAddRequest);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        }
        
        return CreatedAtAction(nameof(GetExchangeValueByID), new {exchangeValueID = res.Value!.Id}, new { res.Value.Id });
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
    public async Task<ActionResult<ExchangeValueResponse>> GetExchangeValueByID(int exchangeValueID)
    {
        var res = await _exchangeValueService.GetExchangeValueByID(exchangeValueID);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        }
        
        return Ok(res.Value);
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
    public async Task<IActionResult> UpdateExchangeValueByID(ExchangeValueUpdateRequest exchangeValueUpdateRequest, int exchangeValueID)
    {
        var res = await _exchangeValueService.UpdateExchangeValueByID(exchangeValueUpdateRequest, exchangeValueID);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
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
    public async Task<IActionResult> DeleteExchangeValueByID(int exchangeValueID)
    {
        var res = await _exchangeValueService.DeleteExchangeValueByID(exchangeValueID);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        }
        
        return Content(res.FirstSuccessMessage()!);
    }
}