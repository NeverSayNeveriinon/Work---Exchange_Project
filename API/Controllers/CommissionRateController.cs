using API.Helpers;
using Core.DTO.CommissionRateDTO;
using Core.Helpers;
using Core.ServiceContracts;
using FluentResults;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Constants.Role.Admin)]
public class CommissionRateController : ControllerBase
{
    private readonly ICommissionRateService _commissionRateService;

    public CommissionRateController(ICommissionRateService commissionRateService)
    {
        _commissionRateService = commissionRateService;
    }
    
    
    /// <summary>
    /// Get All Existing CommissionRates
    /// </summary>
    /// <returns>The CommissionRates List</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/CommissionRate"
    /// 
    /// </remarks>
    /// <response code="200">The CommissionRates List is successfully returned</response>
    [HttpGet]
    // GET: api/CommissionRate
    public async Task<ActionResult<IEnumerable<CommissionRateResponse>>> GetAllCommissionRates()
    {
        var commissionRatesList = await _commissionRateService.GetAllCommissionRates();
        return Ok(commissionRatesList);
    }


    /// <summary>
    /// Add a New CommissionRate to CommissionRates List
    /// </summary>
    /// <returns>Redirect to 'GetCommissionRate' action to return CommissionRate That Has Been Added</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     POST -> "api/CommissionRate"
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">The New CommissionRate is successfully added to CommissionRates List</response>
    /// <response code="400">There is sth wrong in Validation of properties</response>
    [HttpPost]
    // Post: api/CommissionRate
    public async Task<IActionResult> AddCommissionRate(CommissionRateRequest commissionRateRequest)
    {
        var res = await _commissionRateService.AddCommissionRate(commissionRateRequest);
        
        if (res.IsFailed)
            return Problem(res.FirstErrorMessage(), statusCode:400);

        return CreatedAtAction(nameof(GetCommissionRateByMaxRange), new { maxRange = res.Value.MaxUSDRange }, 
                                                                    new { res.Value.Id , res.Value.MaxUSDRange });
    }

    
    /// <summary>
    /// Get an Existing CommissionRate Based On Given maxRange
    /// </summary>
    /// <returns>The CommissionRate That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/CommissionRate/..."
    /// 
    /// </remarks>
    /// <response code="200">The CommissionRate is successfully found and returned</response>
    /// <response code="404">A CommissionRate with Given maxRange has not been found</response>
    [HttpGet("{maxRange:decimal}")]
    // GET: api/CommissionRate/{commissionRateID}
    public async Task<ActionResult<CommissionRateResponse>> GetCommissionRateByMaxRange(decimal maxRange)
    {
        var res = await _commissionRateService.GetCommissionRateByMaxRange(maxRange);
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
    /// Update an Existing CommissionRate Based on Given MaxUSDRange
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Patch -> "api/CommissionRate"
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">The CommissionRate is successfully found and has been updated with New CRate</response>
    /// <response code="404">A CommissionRate with Given MaxUSDRange has not been found</response>
    [HttpPatch]
    // Patch: api/CommissionRate
    public async Task<IActionResult> UpdateCRateByMaxRange(CommissionRateRequest commissionRateRequest)
    {
        var res = await _commissionRateService.UpdateCRateByMaxRange(commissionRateRequest);
        
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
    /// Delete an Existing CommissionRate Based on Given maxRange
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/CommissionRate/..."
    /// 
    /// </remarks>
    /// <response code="204">The CommissionRate is successfully found and has been deleted from CommissionRates List</response>
    /// <response code="404">A CommissionRate with Given maxRange has not been found</response>
    [HttpDelete("{maxRange:decimal}")]
    // Delete: api/CommissionRate/{commissionRateID}
    public async Task<IActionResult> DeleteCommissionRateByMaxRange(decimal maxRange)
    {
        var res = await _commissionRateService.DeleteCommissionRateByMaxRange(maxRange);
        
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