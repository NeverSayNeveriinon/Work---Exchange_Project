using Core.Domain.ExternalServicesContracts;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// TODO: Add Role Logic
// TODO: Add Email Confirm Token Life Time
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notifyService;

    
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService, INotificationService notifyService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _notifyService = notifyService;
    }


    // Register//
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegister userRegister)
    {
        // Validation
        if (ModelState.IsValid == false)
        {
            string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Problem(errorMessage);
        }
        
        var userReturned = await _userManager.FindByEmailAsync(userRegister.Email);
        if (userReturned != null) // means a user with this email is already in db
        {
            return Problem("The Email is Already Registered");
        }
       
        var user = new ApplicationUser()
        {
            UserName = userRegister.Email,
            PersonName = userRegister.PersonName,
            Email = userRegister.Email,
            PhoneNumber = userRegister.Phone
        };

        var result = await _userManager.CreateAsync(user, userRegister.Password);
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
            ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);

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
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return NoContent();
    }
    
    
    // Confirm Email //
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
    
    private async Task SendConfirmationEmail(ApplicationUser? user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"http://localhost:5214/confirm-email?userId={user.Id}&Token={token}";
        await _notifyService.SendAsync(user.Email, "Open and Confirm Your Email", $"Please confirm your account by clicking <a href='{confirmationLink}'>this link</a>;.", true);
    }
}