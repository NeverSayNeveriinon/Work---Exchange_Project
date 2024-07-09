using Core.DTO.CurrencyDTO;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")] 
[ApiController]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    
    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }
    
    [Route("index")]
    [ApiExplorerSettings(IgnoreApi = true)] // For not showing in 'Swagger'
    public IActionResult Index()
    {
        return Content("Here is the \"Currency\" Home Page");
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
    public async Task<ActionResult<IEnumerable<CurrencyResponse>>> GetCurrencies()
    {
        List<CurrencyResponse> currenciesList = await _currencyService.GetAllCurrencies();
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
    public async Task<IActionResult> PostCurrency(CurrencyRequest currencyRequest)
    {
        // No need to do this, because it is done by 'ApiController' attribute in BTS
        // if (!ModelState.IsValid)
        // {
        //     return ValidationProblem(ModelState);
        // }
        
        var currencyResponse = await _currencyService.AddCurrency(currencyRequest);
        
        return CreatedAtAction(nameof(GetCurrency), new {currencyID = currencyResponse.Id}, new { currencyResponse.Id });
    }
    //
    //
    // [HttpGet("CommissionRateRepository")]
    // // Post: api/Currency/CommissionRateRepository
    // public async Task<IActionResult> PostCommissionRate(decimal MaxUSDRange, double CRate)
    // {
    //     // No need to do this, because it is done by 'ApiController' attribute in BTS
    //     // if (!ModelState.IsValid)
    //     // {
    //     //     return ValidationProblem(ModelState);
    //     // }
    //     
    //     var currencyResponse = await _currencyService.AddCurrency(currencyRequest);
    //     
    //     return CreatedAtAction(nameof(GetCurrency), new {currencyID = currencyResponse.Id}, new { currencyResponse.Id });
    // }
    
    
    
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
    public async Task<ActionResult<CurrencyResponse>> GetCurrency(int currencyID)
    {
        CurrencyResponse? currencyObject = await _currencyService.GetCurrencyByID(currencyID);
        if (currencyObject is null)
        {
            return NotFound("notfound:");
        }
        
        return Ok(currencyObject);
    }
    
    
    /// <summary>
    /// Update an Existing Currency Based on Given ID and New Currency Object
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Put -> "api/Currency/..."
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">The Currency is successfully found and has been updated with New Currency</response>
    /// <response code="404">A Currency with Given ID has not been found</response>
    // /// <response code="400">The ID in Url doesn't match with the ID in Body</response>
    [HttpPut("{currencyID:int}")]
    // Put: api/Currency/{currencyID}
    public async Task<IActionResult> PutCurrency(CurrencyRequest currencyRequest, int currencyID)
    {
        // if (currencyID != currencyRequest.n)
        // {
        //     return Problem(detail:"The ID in Url doesn't match with the ID in Body", statusCode:400, title: "Problem With the ID");
        // }

        CurrencyResponse? existingObject = await _currencyService.UpdateCurrency(currencyRequest, currencyID);
        if (existingObject is null)
        {
            return NotFound("notfound:");
        }
        
        return NoContent();
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
    public async Task<IActionResult> DeleteCurrency(int currencyID)
    {
        bool? currencyObject = await _currencyService.DeleteCurrency(currencyID);
        if (currencyObject is null)
        {
            return NotFound("notfound:");
        }

        return NoContent();
    }
}