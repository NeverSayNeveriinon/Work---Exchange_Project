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

    [Route("index")]
    [ApiExplorerSettings(IgnoreApi = true)] // For not showing in 'Swagger'
    public IActionResult Index()
    {
        return Content("Here is the \"CommissionRate\" Home Page");
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
    public async Task<ActionResult<IEnumerable<CommissionRateResponse>>> GetCommissionRates()
    {
        List<CommissionRateResponse> commissionRatesList = await _commissionRateService.GetAllCommissionRates();
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
    public async Task<IActionResult> PostCommissionRate(CommissionRateRequest commissionRateRequest)
    {
        var commissionRateResponse = await _commissionRateService.AddCommissionRate(commissionRateRequest);

        return CreatedAtAction(nameof(GetCommissionRate), new { commissionRateID = commissionRateResponse.Id },
            new { commissionRateResponse.Id });
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
    public async Task<ActionResult<CommissionRateResponse>> GetCommissionRate(int commissionRateID)
    {
        CommissionRateResponse? commissionRateObject =
            await _commissionRateService.GetCommissionRateByID(commissionRateID);
        if (commissionRateObject is null)
        {
            return NotFound("notfound:");
        }

        return Ok(commissionRateObject);
    }


    /// <summary>
    /// Update an Existing CommissionRate Based on Given ID and New CommissionRate Object
    /// </summary>
    /// <returns>Nothing</returns>
    /// <remarks>       
    /// Sample request:
    /// 
    ///     Put -> "api/CommissionRate/..."
    ///     {
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">The CommissionRate is successfully found and has been updated with New CommissionRate</response>
    /// <response code="404">A CommissionRate with Given ID has not been found</response>
    // /// <response code="400">The ID in Url doesn't match with the ID in Body</response>
    // [HttpPut("{commissionRateID:int}")]
    // // Put: api/CommissionRate/{commissionRateID}
    // public async Task<IActionResult> PutCommissionRate(int commissionRateID, CommissionRateRequest commissionRateRequest)
    // {
    //     CommissionRateResponse? existingObject = await _commissionRateService.UpdateCommissionRate(commissionRateID, commissionRateRequest);
    //     if (existingObject is null)
    //     {
    //         return NotFound("notfound:");
    //     }
    //
    //     return NoContent();
    // }


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
    public async Task<IActionResult> DeleteCommissionRate(int commissionRateID)
    {
        bool? commissionRateObject = await _commissionRateService.DeleteCommissionRate(commissionRateID);
        if (commissionRateObject is null)
        {
            return NotFound("notfound:");
        }

        return NoContent();
    }
}