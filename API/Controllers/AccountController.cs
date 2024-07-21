using System.ComponentModel.DataAnnotations;
using Core.Domain.ExternalServicesContracts;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Controllers;

// TODO: Add Email Confirm Token Life Time

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    
    [HttpGet("/")]
    // [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)] // For not showing in 'Swagger'
    public IActionResult Index()
    {
        return Ok("Here is The Home Page");
    }
    
    
    // Register//
    // [Authorize("NotAuthorized")]
    [AllowAnonymous]
    [HttpPost("register")]
    // Post: api/Account/register
    public async Task<IActionResult> Register(UserRegister userRegister)
    {
        if ((User.Identity?.IsAuthenticated ?? false) && !User.IsInRole("Admin"))
            return Unauthorized("You Have Already Logged-In");
        
        if (userRegister.Role == "Admin" && !User.IsInRole("Admin"))
            return Problem("You Are Not Allowed to Create an 'Admin' account", statusCode:403);
        
        var (isValid, message) = await _accountService.Register(userRegister);
        
        if (!isValid)
            return BadRequest(message);
        
        return Ok(message);
    }
    
        
    // Login //
    // [Authorize("NotAuthorized")]
    [AllowAnonymous]
    [HttpPost("login")]
    // Post: api/Account/login
    public async Task<IActionResult> Login(UserLogin userLogin)
    {
        if ((User.Identity?.IsAuthenticated ?? false) && !User.IsInRole("Admin"))
            return Unauthorized("You Have Already Logged-In");
        
        var (isValid, message, obj) = await _accountService.Login(userLogin);

        if (!isValid)
            return BadRequest(message);
        
        return Ok(obj);
    }
    
    
    // Confirm Email //
    // [Authorize("NotAuthorized")]
    [AllowAnonymous]
    [HttpGet("confirm-email")]
    // Get: api/Account/Confirm-Email
    public async Task<IActionResult> ConfirmEmail([BindRequired]Guid userId, string token)
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return Unauthorized("You Have Already Logged-In");
        
        if (!Guid.TryParse(userId.ToString(), out _))
            return BadRequest("User id Is Not In a Correct Format");
        
        if (string.IsNullOrEmpty(token))
            return BadRequest("Token Can't Be Blank");
        
        var (isValid, message) = await _accountService.ConfirmEmail(userId, token);

        if (!isValid)
            return BadRequest(message);
        
        return Ok("Email Has Successfully Confirmed");
    } 
    
    // Confirm Email //
    // [Authorize("NotAuthorized")]
    [AllowAnonymous]
    [HttpGet("send-confirm-email")]
    // Get: api/Account/Confirm-Email
    public async Task<IActionResult> SendConfirmEmail(string userName)
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return Unauthorized("You Have Already Logged-In");
        
        var (isValid, message) = await _accountService.SendConfirmationEmail(userName);

        if (!isValid)
            return BadRequest(message);
        
        return Ok(message);
    }
    
    
    [HttpPost("Defined-Accounts")]
    [Authorize]
    // Post: api/Account/Defined-Accounts
    public async Task<IActionResult> AddDefinedAccount([Length(10,10)]string definedAccountAddNumber)
    {
        var (isValid, message) = await _accountService.AddDefinedAccount(definedAccountAddNumber, User.Identity!.Name!);

        if (!isValid)
            return BadRequest(message);
        
        return Ok(message);
    }
    
    
            
    // Logout //
    // // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // [HttpGet("logout")]
    // public async Task<IActionResult> Logout()
    // {
    //     var serviceResult = await _accountService.Logout();
    //     return Ok(serviceResult.Message);
    // }
}