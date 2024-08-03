using System.ComponentModel.DataAnnotations;
using Core.DTO.CurrencyDTO;
using Core.Helpers;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")] 
[ApiController]
[Authorize(Roles = Constants.Role.Admin)]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    
    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }
    
    /// <summary>
    /// Get All Existing Currencies
    /// </summary>
    /// <returns>The Currencies List</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/Currency"
    /// 
    /// </remarks>
    /// <response code="200">The Currencies List is successfully returned</response>
    [HttpGet]
    // GET: api/Currency
    public async Task<ActionResult<IEnumerable<CurrencyResponse>>> GetAllCurrencies()
    {
        var currenciesList = await _currencyService.GetAllCurrencies();
        return Ok(currenciesList);
    }
    
     
    /// <summary>
    /// Add a New Currency to Currencies List
    /// </summary>
    /// <returns>Redirect to 'GetCurrency' action to return Currency That Has Been Added</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     POST -> "api/Currency"
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The New Currency is successfully added to Currencies List</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [HttpPost]
    // Post: api/Currency
    public async Task<IActionResult> AddCurrency(CurrencyRequest currencyRequest)
    {
        var res = await _currencyService.AddCurrency(currencyRequest);
        if (res.IsFailed)
            return Problem(res.FirstErrorMessage(), statusCode:400);
        
        return CreatedAtAction(nameof(GetCurrencyByID), new {currencyID = res.Value.Id}, new { res.Value.Id });
    }

    
    
    /// <summary>
    /// Get an Existing Currency Based On Given ID
    /// </summary>
    /// <returns>The Currency That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/Currency/..."
    /// 
    /// </remarks>
    /// <response code="200">The Currency is successfully found and returned</response>
    /// <response code="404">A Currency with Given ID has not been found</response>
    [HttpGet("{currencyID:int}")]
    // GET: api/Currency/{currencyID}
    public async Task<ActionResult<CurrencyResponse>> GetCurrencyByID(int currencyID)
    {
        var res = await _currencyService.GetCurrencyByID(currencyID);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode: 400);
        }
        
        return Ok(res.Value);
    }    
    
    /// <summary>
    /// Get an Existing Currency Based On Given currencyType
    /// </summary>
    /// <returns>The Currency That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/Currency/..."
    /// 
    /// </remarks>
    /// <response code="200">The Currency is successfully found and returned</response>
    /// <response code="404">A Currency with Given currencyType has not been found</response>
    [HttpGet("{currencyType}")]
    // GET: api/Currency/{currencyType}
    public async Task<ActionResult<CurrencyResponse>> GetCurrencyByCurrencyType([RegularExpression("^[A-Z]{3}$")]string currencyType)
    {
        var res = await _currencyService.GetCurrencyByCurrencyType(currencyType);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode: 400);
        }
        
        return Ok(res.Value);
    }
    
    
    /// <summary>
    /// Delete an Existing Currency Based on Given ID
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/Currency/..."
    /// 
    /// </remarks>
    /// <response code="204">The Currency is successfully found and has been deleted from Currencies List</response>
    /// <response code="404">A Currency with Given ID has not been found</response>
    [HttpDelete("{currencyID:int}")]
    // Delete: api/Currency/{currencyID}
    public async Task<IActionResult> DeleteCurrencyByID(int currencyID)
    {
        var res = await _currencyService.DeleteCurrencyByID(currencyID);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode: 400);
        }
        
        return Content(res.FirstSuccessMessage()!);
    }
}