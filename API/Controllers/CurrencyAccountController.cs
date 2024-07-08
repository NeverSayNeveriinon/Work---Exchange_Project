using Core.DTO.CurrencyAccountDTO;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")] 
[ApiController]
public class CurrencyAccountController : ControllerBase
{
    private readonly ICurrencyAccountService _currencyAccountService;
    
    public CurrencyAccountController(ICurrencyAccountService currencyAccountService)
    {
        _currencyAccountService = currencyAccountService;
    }
    
    [Route("/")]
    [ApiExplorerSettings(IgnoreApi = true)] // For not showing in 'Swagger'
    public IActionResult Index()
    {
        return Content("Here is the \"CurrencyAccount\" Home Page");
    }
    
    
    
    /// <summary>
    /// Get All Existing CurrencyAccounts
    /// </summary>
    /// <returns>The CurrencyAccounts List</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/CurrencyAccount"
    /// 
    /// </remarks>
    /// <response code="200">The CurrencyAccounts List is successfully returned</response>
    [HttpGet]
    // GET: api/CurrencyAccount
    public async Task<ActionResult<IEnumerable<CurrencyAccountResponse>>> GetCurrencyAccounts()
    {
        List<CurrencyAccountResponse> currencyAccountsList = await _currencyAccountService.GetAllCurrencyAccounts();
        return Ok(currencyAccountsList);
    }
    
     
    /// <summary>
    /// Add a New CurrencyAccount to CurrencyAccounts List
    /// </summary>
    /// <returns>Redirect to 'GetCurrencyAccount' action to return CurrencyAccount That Has Been Added</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     POST -> "api/CurrencyAccount"
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The New CurrencyAccount is successfully added to CurrencyAccounts List</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [HttpPost]
    // Post: api/CurrencyAccount
    public async Task<IActionResult> PostCurrencyAccount(CurrencyAccountRequest currencyAccount)
    {
        // No need to do this, because it is done by 'ApiController' attribute in BTS
        // if (!ModelState.IsValid)
        // {
        //     return ValidationProblem(ModelState);
        // }
        
        await _currencyAccountService.AddCurrencyAccount(currencyAccount);
        
        return CreatedAtAction(nameof(GetCurrencyAccount), new {currencyAccountID = currencyAccount}, currencyAccount);
    }
    
    
    
    /// <summary>
    /// Get an Existing CurrencyAccount Based On Given ID
    /// </summary>
    /// <returns>The CurrencyAccount That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/CurrencyAccount/..."
    /// 
    /// </remarks>
    /// <response code="200">The CurrencyAccount is successfully found and returned</response>
    /// <response code="404">A CurrencyAccount with Given ID has not been found</response>
    [HttpGet("{currencyAccountID:int}")]
    // GET: api/CurrencyAccount/{currencyAccountID}
    public async Task<ActionResult<CurrencyAccountResponse>> GetCurrencyAccount(int currencyAccountID)
    {
        CurrencyAccountResponse? currencyAccountObject = await _currencyAccountService.GetCurrencyAccountByID(currencyAccountID);
        if (currencyAccountObject is null)
        {
            return NotFound("notfound:");
        }
        
        return Ok(currencyAccountObject);
    }
    
    
    /// <summary>
    /// Update an Existing CurrencyAccount Based on Given ID and New CurrencyAccount Object
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Put -> "api/CurrencyAccount/..."
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">The CurrencyAccount is successfully found and has been updated with New CurrencyAccount</response>
    /// <response code="404">A CurrencyAccount with Given ID has not been found</response>
    // /// <response code="400">The ID in Url doesn't match with the ID in Body</response>
    [HttpPut("{currencyAccountID:int}")]
    // Put: api/CurrencyAccount/{currencyAccountID}
    public async Task<IActionResult> PutCurrencyAccount(CurrencyAccountRequest currencyAccountRequest, int currencyAccountID)
    {
        // if (currencyAccountID != currencyAccountRequest.n)
        // {
        //     return Problem(detail:"The ID in Url doesn't match with the ID in Body", statusCode:400, title: "Problem With the ID");
        // }

        CurrencyAccountResponse? existingObject = await _currencyAccountService.UpdateCurrencyAccount(currencyAccountRequest, currencyAccountID);
        if (existingObject is null)
        {
            return NotFound("notfound:");
        }
        
        return NoContent();
    }
    
    
    /// <summary>
    /// Delete an Existing CurrencyAccount Based on Given ID
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/CurrencyAccount/..."
    /// 
    /// </remarks>
    /// <response code="204">The CurrencyAccount is successfully found and has been deleted from CurrencyAccounts List</response>
    /// <response code="404">A CurrencyAccount with Given ID has not been found</response>
    [HttpDelete("{currencyAccountID:int}")]
    // Delete: api/CurrencyAccount/{currencyAccountID}
    public async Task<IActionResult> DeleteCurrencyAccount(int currencyAccountID)
    {
        bool? currencyAccountObject = await _currencyAccountService.DeleteCurrencyAccount(currencyAccountID);
        if (currencyAccountObject is null)
        {
            return NotFound("notfound:");
        }

        return NoContent();
    }
}