using Core.Domain.ExternalServicesContracts;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Index()
    {
        return Ok("Here is The Home Page");
    }
    
    
    // Register//
    [Authorize("NotAuthorized")]
    [HttpPost("register")]
    // Post: api/Account/register
    public async Task<IActionResult> Register(UserRegister userRegister)
    {
        var (isValid, message) = await _accountService.Register(userRegister);
        
        if (!isValid)
            return BadRequest(message);
        
        return Ok(message);
    }
    
        
    // Login //
    [Authorize("NotAuthorized")]
    [HttpPost("login")]
    // Post: api/Account/login
    public async Task<IActionResult> Login(UserLogin loginDTO)
    {
        var (isValid, message, obj) = await _accountService.Login(loginDTO);

        if (!isValid)
            return BadRequest(message);
        
        return Ok(obj);
    }
    
    
    // Confirm Email //
    [Authorize("NotAuthorized")]
    [HttpGet("confirm-email")]
    // Get: api/Account/Confirm-Email
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var (isValid, message, obj) = await _accountService.ConfirmEmail(userId, token);

        if (!isValid)
            return BadRequest(message);
        
        return Ok(obj);
    }
    
    
    
    [HttpPost("Defined-Accounts")]
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    // Post: api/Account/Defined-Accounts
    public async Task<IActionResult> AddDefinedAccount(string definedAccountAddNumber)
    {
        var (isValid, message) = await _accountService.AddDefinedAccount(definedAccountAddNumber, User);

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