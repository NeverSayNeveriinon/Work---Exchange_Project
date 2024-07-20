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
    public async Task<ActionResult<IEnumerable<CurrencyAccountResponse>>> GetAllCurrencyAccounts()
    {
        var currencyAccountsList = await _currencyAccountService.GetAllCurrencyAccounts(User);
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
    [HttpPost]
    // Post: api/CurrencyAccount
    public async Task<IActionResult> AddCurrencyAccount(CurrencyAccountAddRequest currencyAccountAdd)
    {
        bool isValid = await _validator.ExistsInCurrentCurrencies(currencyAccountAdd.CurrencyType) &&
                       await _validator.ExistsInCurrentCurrencies(currencyAccountAdd.MoneyToOpenAccount.CurrencyType);
        if (!isValid)
        {
            ModelState.AddModelError("CurrencyType", "The CurrencyType is not in Current Currencies");
            return BadRequest(ModelState);
        }
        
        var (isValidCurrencyAccount, message, currencyAccountResponse) = await _currencyAccountService.AddCurrencyAccount(currencyAccountAdd, User);
        if (!isValidCurrencyAccount)
            return BadRequest(message);
        
        return CreatedAtAction(nameof(GetCurrencyAccountByNumber), new {currencyAccountNumber = currencyAccountResponse!.Number}, new { currencyAccountResponse.Number });
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
    [HttpGet]
    // GET: api/CurrencyAccount/{currencyAccountNumber}
    public async Task<ActionResult<CurrencyAccountResponse>> GetCurrencyAccountByNumber(string currencyAccountNumber)
    {
        var (isValid, message, currencyAccountResponse) = await _currencyAccountService.GetCurrencyAccountByNumber(currencyAccountNumber, User);
        
        if (!isValid && message is null)
            return NotFound("!!A Currency Account With This Number Has Not Been Found!!");
        
        if (!isValid)
            return BadRequest(message);
        
        return Ok(currencyAccountResponse);
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
        var (isValid, isFound, message) = await _currencyAccountService.DeleteCurrencyAccount(currencyAccountNumber, User);
        
        if (!isValid && !isFound)
            return NotFound("!!A Currency Account With This Number Has Not Been Found!!");
        if (!isValid)
            return BadRequest(message);
        
        return NoContent();
    }
}