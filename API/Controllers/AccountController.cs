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

// TODO: Add Role Logic
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
    
    // Register//
    [Authorize("NotAuthorized")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegister userRegister)
    {
        var serviceResult = await _accountService.Register(userRegister);
        
        if (serviceResult.isValid)
            return Ok(serviceResult.Message);
        else
            return BadRequest(serviceResult.Message);
    }
    
        
    // Login //
    [Authorize("NotAuthorized")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLogin loginDTO)
    {
        var serviceResult = await _accountService.Login(loginDTO);
        
        if (serviceResult.isValid)
            return Ok(serviceResult.obj);
        else
            return BadRequest(serviceResult.Message);
    }
        
        
    // // Logout //
    // // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // [HttpGet("logout")]
    // public async Task<IActionResult> Logout()
    // {
    //     var serviceResult = await _accountService.Logout();
    //     return Ok(serviceResult.Message);
    // }
    
    
    // Confirm Email //
    [Authorize("NotAuthorized")]
    [HttpGet("/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var serviceResult = await _accountService.ConfirmEmail(userId, token);
        
        if (serviceResult.isValid)
            return Ok(serviceResult.obj);
        else
            return BadRequest(serviceResult.Message);
    }
    
    // Confirm Email //
    [HttpGet("/")]
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Index()
    {
        return Ok("Ooooo");
    }
    
    
    [HttpPost("Defined-Accounts")]
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    // Post: api/Account/Defined-Accounts
    public async Task<IActionResult> PostDefinedAccount(int definedAccountAddNumber)
    {
        var serviceResult = await _accountService.AddDefinedAccount(definedAccountAddNumber, User);
        
        if (serviceResult.isValid)
            return Ok(serviceResult.Message);
        else
            return BadRequest(serviceResult.Message);
    }
}