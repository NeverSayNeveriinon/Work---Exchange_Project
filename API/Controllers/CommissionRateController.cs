using Core.DTO.CommissionRateDTO;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
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
        var commissionRateResponse = await _commissionRateService.AddCommissionRate(commissionRateRequest);

        return CreatedAtAction(nameof(GetCommissionRateByID), new { commissionRateID = commissionRateResponse.Id }, new { commissionRateResponse.Id });
    }

    
    /// <summary>
    /// Get an Existing CommissionRate Based On Given ID
    /// </summary>
    /// <returns>The CommissionRate That Has Been Found</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Get -> "api/CommissionRate/..."
    /// 
    /// </remarks>
    /// <response code="200">The CommissionRate is successfully found and returned</response>
    /// <response code="404">A CommissionRate with Given ID has not been found</response>
    [HttpGet("{commissionRateID:int}")]
    // GET: api/CommissionRate/{commissionRateID}
    public async Task<ActionResult<CommissionRateResponse>> GetCommissionRateByID(int commissionRateID)
    {
        var commissionRateResponse = await _commissionRateService.GetCommissionRateByID(commissionRateID);
        if (commissionRateResponse is null)
        {
            return NotFound("!!A Commission Rate With This ID Has Not Been Found!!");
        }

        return Ok(commissionRateResponse);
    }

    
    /// <summary>
    /// Delete an Existing CommissionRate Based on Given ID
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Delete -> "api/CommissionRate/..."
    /// 
    /// </remarks>
    /// <response code="204">The CommissionRate is successfully found and has been deleted from CommissionRates List</response>
    /// <response code="404">A CommissionRate with Given ID has not been found</response>
    [HttpDelete("{commissionRateID:int}")]
    // Delete: api/CommissionRate/{commissionRateID}
    public async Task<IActionResult> DeleteCommissionRateByID(int commissionRateID)
    {
        bool? commissionRateResponse = await _commissionRateService.DeleteCommissionRate(commissionRateID);
        if (commissionRateResponse is null)
        {
            return NotFound("!!A Commission Rate With This ID Has Not Been Found!!");
        }

        return NoContent();
    }
}