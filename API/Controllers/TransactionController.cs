using System.ComponentModel.DataAnnotations;
using Core.DTO.TransactionDTO;
using Core.Enums;
using Core.Helpers;
using Core.ServiceContracts;
using FluentResults.Extensions.AspNetCore;
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
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetAllTransactions()
    {
        var res = await _transactionService.GetAllTransactions(User);
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
    /// <response code="200">The New Transaction is successfully added to Transactions List, Waiting For Confirm</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [Idempotent(ExpireHours = 1)]
    [HttpPost("Transfer")]
    // Post: api/Transaction/Transfer/{transactionAddRequest}
    public async Task<IActionResult> AddTransferTransaction(TransactionTransferAddRequest transactionAddRequest, [Required][FromHeader]string? IdempotencyKey)
    {
        if (string.IsNullOrEmpty(IdempotencyKey))
            return Problem("You Must Set 'IdempotencyKey' in Header", statusCode:400);
        
        var res = await _transactionService.AddTransferTransaction(transactionAddRequest, User);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        }
        
        return Ok(res.Value);
        // return CreatedAtAction(nameof(GetTransactionByID), new {transactionID = transactionResponse.Id}, new { transactionResponse.Id });
        // return Ok("Please Confirm The Transaction");
        // return RedirectToRoute(nameof(ConfirmTransaction), new { transactionId = transactionResponse.Id, isConfirmed = true });
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
    /// <response code="200">The New Transaction is successfully added to Transactions List, Waiting For Confirm/Cancel</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [Idempotent(ExpireHours = 1)]
    [HttpPost("Balance-Deposit")]
    // Post: api/Transaction/Balance-Deposit/{transactionAddRequest}
    public async Task<IActionResult> AddDepositTransaction(TransactionDepositAddRequest transactionAddRequest, [Required][FromHeader]string? IdempotencyKey)
    {
        if (string.IsNullOrEmpty(IdempotencyKey))
            return Problem("You Must Set 'IdempotencyKey' in Header", statusCode:400);
        
        bool isValid = await _validator.ExistsInCurrentCurrencies(transactionAddRequest.Money.CurrencyType);
        if (!isValid)
        {
            ModelState.AddModelError("CurrencyType", "The CurrencyType is not in Current Currencies");
            return new BadRequestObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext,ModelState));
        }
        
        var res = await _transactionService.AddDepositTransaction(transactionAddRequest, User);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        }
        
        return Ok(res.Value);
        // return Ok("Please Confirm The Transaction");
        // return CreatedAtAction(nameof(GetTransactionByID), new {transactionID = transactionResponse.Id}, new { transactionResponse.Id });
        // return RedirectToAction(nameof(ConfirmTransaction), new { transactionId = transactionResponse.Id, isConfirmed = true });
    } 
    
    
    /// <summary>
    /// Confirm/Cancel a Pending Transaction Based On Given ID
    /// </summary>
    /// <returns>Redirect to 'GetTransactionByID' action to return Transaction That Has Been Updated</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Patch -> "api/Transaction"
    ///     {
    ///       "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "transactionStatus": "Confirmed|Cancelled"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The Transaction is successfully found and Updated</response>
    /// <response code="404">A Transaction with Given ID has not been found</response>
    [HttpPatch("ChangeStatus")]
    // Post: api/Transaction/ChangeStatus
    public async Task<IActionResult> ChangeTransactionStatus(ConfirmTransactionRequest confirmTransactionRequest)
    {
        var res = await _transactionService.UpdateTransactionStatusOfTransaction(confirmTransactionRequest, User, DateTime.Now);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        }
        
        return CreatedAtAction(nameof(GetTransactionByID), new {transactionID = res.Value.Id}, new { res.Value.Id });
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
    [HttpGet("{transactionID:guid}")]
    // GET: api/Transaction/{transactionID}
    public async Task<ActionResult<TransactionResponse>> GetTransactionByID(Guid transactionID)
    {
        var res = await _transactionService.GetTransactionByID(transactionID, User);
        if (res.IsFailed)
        {
            var error = res.Errors.FirstOrDefault();
            if (error.IsStatusCode(nameof(StatusCodes.Status404NotFound)))
                return Problem(error?.Message, statusCode:404);

            return Problem(error?.Message, statusCode:400);
        };
        
        return Ok(res.Value);
    }
    
    
    // /// <summary>
    // /// Delete an Existing Transaction Based on Given ID
    // /// </summary>
    // /// <returns>Nothing</returns>
    // /// <remarks>       
    // /// Sample request:
    // /// 
    // ///     Delete -> "api/Transaction/..."
    // /// 
    // /// </remarks>
    // /// <response code="204">The Transaction is successfully found and has been deleted from Transactions List</response>
    // /// <response code="404">A Transaction with Given ID has not been found</response>
    // [HttpDelete("{transactionID:guid}")]
    // // Delete: api/Transaction/{transactionID}
    // public async Task<IActionResult> DeleteTransactionById(Guid transactionID)
    // {
    //     var (isValid, isFound, message) = await _transactionService.DeleteTransactionById(transactionID, User);
    //
    //     if (!isValid && !isFound)
    //         return NotFound("!!A Transaction With This ID Has Not Been Found!!");
    //     if (!isValid)
    //         return Problem(message, statusCode:400);
    //     
    //     return NoContent();
    // }
}