using Core.DTO;
using Core.DTO.CurrencyAccountDTO;
using Core.DTO.TransactionDTO;
using Core.Helpers;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")] 
[ApiController]
[Authorize]
public class CurrencyAccountController : ControllerBase
{
    private readonly ICurrencyAccountService _currencyAccountService;
    private readonly IValidator _validator;
    
    public CurrencyAccountController(ICurrencyAccountService currencyAccountService, IValidator validator)
    {
        _currencyAccountService = currencyAccountService;
        _validator = validator;
    }
    
    [Route("index")]
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
    ///         CurrencyType = "USD"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The New CurrencyAccount is successfully added to CurrencyAccounts List</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [HttpPost("moneyToOpenAccount:decimal")]
    // Post: api/CurrencyAccount/{moneyToOpenAccount}
    public async Task<IActionResult> PostCurrencyAccount(CurrencyAccountAddRequest currencyAccountAdd)
    {
        bool isValid = await _validator.ExistsInCurrentCurrencies(currencyAccountAdd.CurrencyType) &&
                       await _validator.ExistsInCurrentCurrencies(currencyAccountAdd.MoneyToOpenAccount.CurrencyType);
        if (!isValid)
        {
            ModelState.AddModelError("CurrencyType", "The CurrencyType is not in Current Currencies");
            return BadRequest(ModelState);
        }
        
        
        var currencyAccountResponse = await _currencyAccountService.AddCurrencyAccount(currencyAccountAdd, User);
        
        return CreatedAtAction(nameof(GetCurrencyAccount), new {currencyAccountNumber = currencyAccountResponse.Number}, new { currencyAccountResponse.Number });
    }    
    
    
    /// <summary>
    /// Get an Existing CurrencyAccount Based On Given Number
    /// </summary>
    /// <returns>The CurrencyAccount That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/CurrencyAccount/..."
    /// 
    /// </remarks>
    /// <response code="200">The CurrencyAccount is successfully found and returned</response>
    /// <response code="404">A CurrencyAccount with Given Number has not been found</response>
    [HttpGet("{currencyAccountNumber:int}")]
    // GET: api/CurrencyAccount/{currencyAccountNumber}
    public async Task<ActionResult<CurrencyAccountResponse>> GetCurrencyAccount(string currencyAccountNumber)
    {
        CurrencyAccountResponse? currencyAccountObject = await _currencyAccountService.GetCurrencyAccountByNumber(currencyAccountNumber);
        if (currencyAccountObject is null)
        {
            return NotFound("notfound:");
        }
        
        return Ok(currencyAccountObject);
    }
    
    
    /// <summary>
    /// Update an Existing CurrencyAccount Based on Given Number and New CurrencyAccount Object
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
    /// <response code="404">A CurrencyAccount with Given Number has not been found</response>
    // /// <response code="400">The Number in Url doesn't match with the Number in Body</response>
    [HttpPut]
    // Put: api/CurrencyAccount/{currencyAccountNumber}
    public async Task<IActionResult> PutCurrencyAccount(CurrencyAccountUpdateRequest currencyAccountUpdateRequest, string currencyAccountNumber)
    {
        bool isValid = await _validator.ExistsInCurrentCurrencies(currencyAccountUpdateRequest.CurrencyType);
        if (!isValid)
        {
            ModelState.AddModelError("CurrencyType", "The CurrencyType is not in Current Currencies");
            return BadRequest(ModelState);
        }
        
        CurrencyAccountResponse? existingObject = await _currencyAccountService.UpdateCurrencyAccount(currencyAccountUpdateRequest, currencyAccountNumber);
        if (existingObject is null)
        {
            return NotFound("notfound:");
        }
        
        return NoContent();
    }
    
    
    /// <summary>
    /// Delete an Existing CurrencyAccount Based on Given Number
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/CurrencyAccount/..."
    /// 
    /// </remarks>
    /// <response code="204">The CurrencyAccount is successfully found and has been deleted from CurrencyAccounts List</response>
    /// <response code="404">A CurrencyAccount with Given Number has not been found</response>
    [HttpDelete]
    // Delete: api/CurrencyAccount/{currencyAccountNumber}
    public async Task<IActionResult> DeleteCurrencyAccount(string currencyAccountNumber)
    {
        bool? currencyAccountObject = await _currencyAccountService.DeleteCurrencyAccount(currencyAccountNumber);
        if (currencyAccountObject is null)
        {
            return NotFound("notfound:");
        }

        return NoContent();
    }
}