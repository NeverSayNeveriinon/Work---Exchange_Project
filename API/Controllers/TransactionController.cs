using Core.DTO.TransactionDTO;
using Core.ServiceContracts;
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
    
    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
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
    [HttpPost]
    // Post: api/Transaction
    public async Task<IActionResult> PostTransaction(TransactionAddRequest transactionAddRequest)
    {
        var transactionResponse = await _transactionService.AddTransaction(transactionAddRequest);
        
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