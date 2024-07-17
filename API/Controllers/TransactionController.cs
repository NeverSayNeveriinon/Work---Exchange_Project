using Core.DTO.TransactionDTO;
using Core.Helpers;
using Core.ServiceContracts;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")] 
[ApiController]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IValidator _validator ;
    
    public TransactionController(ITransactionService transactionService, IValidator validator)
    {
        _transactionService = transactionService;
        _validator = validator;
    }
    
    [Route("index")]
    [ApiExplorerSettings(IgnoreApi = true)] // For not showing in 'Swagger'
    public IActionResult Index()
    {
        return Content("Here is the \"Transaction\" Home Page");
    }
    
    
    
    /// <summary>
    /// Get All Existing Transactions
    /// </summary>
    /// <returns>The Transactions List</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/Transaction"
    /// 
    /// </remarks>
    /// <response code="200">The Transactions List is successfully returned</response>
    [HttpGet]
    // GET: api/Transaction
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactions()
    {
        List<TransactionResponse> transactionsList = await _transactionService.GetAllTransactions();
        return Ok(transactionsList);
    }
    
     
    /// <summary>
    /// Add a New Transaction to Transactions List
    /// </summary>
    /// <returns>Redirect to 'GetTransaction' action to return Transaction That Has Been Added</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     POST -> "api/Transaction"
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The New Transaction is successfully added to Transactions List</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [Idempotent(ExpireHours = 1)]
    [HttpPost("Transfer")]
    // Post: api/Transaction/Transfer/{transactionAddRequest}
    public async Task<IActionResult> AddTransferTransaction(TransactionTransferAddRequest transactionAddRequest, [FromHeader]string? IdempotencyKey)
    {
        if (string.IsNullOrEmpty(IdempotencyKey))
        {
            return BadRequest("You Must Set 'IdempotencyKey' in Header");
        }
        var transactionResponse = await _transactionService.AddTransferTransaction(transactionAddRequest, User);
        
        // return CreatedAtAction(nameof(GetTransaction), new {transactionID = transactionResponse.Id}, new { transactionResponse.Id });
        return Ok("Please Confirm The Transaction");
    }
    
    [Idempotent(ExpireHours = 1)]
    [HttpPost("Balance-Increase")]
    // Post: api/Transaction/Balance-Increase/{transactionAddRequest}
    public async Task<IActionResult> AddDepositTransaction(TransactionDepositAddRequest transactionAddRequest, [FromHeader]string? IdempotencyKey)
    {
        if (string.IsNullOrEmpty(IdempotencyKey))
        {
            return BadRequest("You Must Set 'IdempotencyKey' in Header");
        }
        
        bool isValid = await _validator.ExistsInCurrentCurrencies(transactionAddRequest.Money.CurrencyType);
        if (!isValid)
        {
            ModelState.AddModelError("CurrencyType", "The CurrencyType is not in Current Currencies");
            return BadRequest(ModelState);
        }
        
        var transactionResponse = await _transactionService.AddDepositTransaction(transactionAddRequest);
        
        // return CreatedAtAction(nameof(GetTransaction), new {transactionID = transactionResponse.Id}, new { transactionResponse.Id });
        return Ok("Please Confirm The Transaction");
    } 
    
    [HttpPatch("Confirm")]
    // Post: api/Transaction/Confirm/{transactionId}
    public async Task<IActionResult> ConfirmTransaction(int transactionId, bool isConfirmed)
    {
        var transactionResponse = await _transactionService.UpdateIsConfirmedOfTransaction(transactionId, isConfirmed, DateTime.Now.TimeOfDay);
        if (transactionResponse is null)
        {
            return NotFound("notfound:");
        }
        
        return CreatedAtAction(nameof(GetTransaction), new {transactionID = transactionResponse.Id}, new { transactionResponse.Id });
    }
    
    
    /// <summary>
    /// Get an Existing Transaction Based On Given ID
    /// </summary>
    /// <returns>The Transaction That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/Transaction/..."
    /// 
    /// </remarks>
    /// <response code="200">The Transaction is successfully found and returned</response>
    /// <response code="404">A Transaction with Given ID has not been found</response>
    [HttpGet("{transactionID:int}")]
    // GET: api/Transaction/{transactionID}
    public async Task<ActionResult<TransactionResponse>> GetTransaction(int transactionID)
    {
        TransactionResponse? transactionObject = await _transactionService.GetTransactionByID(transactionID);
        if (transactionObject is null)
        {
            return NotFound("notfound:");
        }
        
        return Ok(transactionObject);
    }
    
    
    /// <summary>
    /// Update an Existing Transaction Based on Given ID and New Transaction Object
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Put -> "api/Transaction/..."
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">The Transaction is successfully found and has been updated with New Transaction</response>
    /// <response code="404">A Transaction with Given ID has not been found</response>
    // /// <response code="400">The ID in Url doesn't match with the ID in Body</response>
    [HttpPut("{transactionID:int}")]
    // Put: api/Transaction/{transactionID}
    public async Task<IActionResult> PutTransaction(TransactionUpdateRequest transactionUpdateRequest, int transactionID)
    {
        TransactionResponse? existingObject = await _transactionService.UpdateTransaction(transactionUpdateRequest, transactionID);
        if (existingObject is null)
        {
            return NotFound("notfound:");
        }
        
        return NoContent();
    }
    
    
    /// <summary>
    /// Delete an Existing Transaction Based on Given ID
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/Transaction/..."
    /// 
    /// </remarks>
    /// <response code="204">The Transaction is successfully found and has been deleted from Transactions List</response>
    /// <response code="404">A Transaction with Given ID has not been found</response>
    [HttpDelete("{transactionID:int}")]
    // Delete: api/Transaction/{transactionID}
    public async Task<IActionResult> DeleteTransaction(int transactionID)
    {
        bool? transactionObject = await _transactionService.DeleteTransaction(transactionID);
        if (transactionObject is null)
        {
            return NotFound("notfound:");
        }

        return NoContent();
    }
}