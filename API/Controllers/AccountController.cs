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
// TODO: Add Defined Accounts

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<UserProfile> _userManager;
    private readonly SignInManager<UserProfile> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notifyService;
    
    public AccountController(UserManager<UserProfile> userManager, SignInManager<UserProfile> signInManager, IJwtService jwtService, INotificationService notifyService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _notifyService = notifyService;
    }
    
    // Register//
    [Authorize("NotAuthorized")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegister userRegister)
    {
        var userReturned = await _userManager.FindByEmailAsync(userRegister.Email);
        if (userReturned != null) // if userReturned has sth, means a user with this email is already in db
        {
            return Problem("The Email is Already Registered");
        }
       
        var user = new UserProfile()
        {
            UserName = userRegister.Email,
            PersonName = userRegister.PersonName,
            Email = userRegister.Email,
            PhoneNumber = userRegister.Phone,
            DefinedAccountNumbers = new List<int>()
        };

        var result = await _userManager.CreateAsync(user, userRegister.Password);
        await _userManager.AddToRoleAsync(user, userRegister.Role);
        if (result.Succeeded)
        {
            await SendConfirmationEmail(user);
            return Ok("Please Check Your Email");
        }
        else
        {
            string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description)); //error1 | error2
            return Problem(errorMessage);
        }   
    }
    
        
    // Login //
    [Authorize("NotAuthorized")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLogin loginDTO)
    {
        // Validation
        if (ModelState.IsValid == false)
        {
            string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Problem(errorMessage);
        }
        
        var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            UserProfile? user = await _userManager.FindByEmailAsync(loginDTO.Email);
            
            if (user == null)
            {
                return NoContent();
            }
            
            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            var authenticationResponse = _jwtService.CreateJwtToken(user);

            return Ok(authenticationResponse);
        }

        else
        {
            return Problem("Invalid email or password");
        }
    }
        
        
    // Logout //
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return NoContent();
    }
    
    
    // Confirm Email //
    [Authorize("NotAuthorized")]
    [HttpGet("/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (userId == null || token == null)
        {
            return Problem("Link expired");
        }
        else if (user == null)
        {
            return Problem("User not Found");
        }
        else
        {
            token = token.Replace(" ", "+");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                var authenticationResponse = _jwtService.CreateJwtToken(user);
                return Ok(authenticationResponse);
            }
            else
            {
                return Problem("Email not confirmed");
            }
        }
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
    public async Task<IActionResult> PostDefinedAccount(int definedAccountAddID)
    {
        var existingUser = await _userManager.GetUserAsync(User);
        if (existingUser == null)
        {
            return Problem("Try to Log-In Again,if it doesn't worked it seems you haven't signed up!!");
        }
        
        existingUser.DefinedAccountNumbers!.Add(definedAccountAddID);
        
        var resultIdentity = await _userManager.UpdateAsync(existingUser);
        if (!resultIdentity.Succeeded)
        {
            return Problem("The Defined Account ID has not been Added, Try Again");
        }

        
        return NoContent();
    }
    
    
    
    
    
    
    
    
    private async Task SendConfirmationEmail(UserProfile? user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"http://localhost:5214/confirm-email?userId={user.Id}&Token={token}";
        await _notifyService.SendAsync(user.Email, "Open and Confirm Your Email", $"Please confirm your account by clicking <a href='{confirmationLink}'>this link</a>;.", true);
    }
}