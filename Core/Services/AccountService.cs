using System.Security.Claims;
using Core.Domain.ExternalServicesContracts;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Core.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<UserProfile> _userManager;
    private readonly RoleManager<UserRole> _roleManager;
    private readonly SignInManager<UserProfile> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notifyService;
    
    public AccountService(UserManager<UserProfile> userManager, SignInManager<UserProfile> signInManager, IJwtService jwtService, INotificationService notifyService, RoleManager<UserRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _notifyService = notifyService;
        _roleManager = roleManager;
    }
    
    
    public async Task<(bool isValid, string Message)> Register(UserRegister userRegister)
    {
        var userReturned = await _userManager.FindByEmailAsync(userRegister.Email);
        if (userReturned != null) // if userReturned has sth, means a user with this email is already in db
            return (false, "The Email is Already Registered");
       
        var user = new UserProfile()
        {
            UserName = userRegister.Email,
            PersonName = userRegister.PersonName,
            Email = userRegister.Email,
            PhoneNumber = userRegister.Phone,
            DefinedAccountNumbers = new List<int>()
        };
        
        var result = await _userManager.CreateAsync(user, userRegister.Password);
        
        if (!await _roleManager.RoleExistsAsync(userRegister.Role))
        {
            UserRole userRole = new UserRole() { Name = userRegister.Role };
            await _roleManager.CreateAsync(userRole);
            await _roleManager.AddClaimAsync(userRole,new Claim(ClaimTypes.Role, userRole.Name));
        }
        await _userManager.AddClaimAsync(user,new Claim(ClaimTypes.Role, userRegister.Role));
        await _userManager.AddToRoleAsync(user, userRegister.Role);
        
        if (result.Succeeded)
        {
            await SendConfirmationEmail(user);
            return (true, "Please Check Your Email");
        }
        else
        {
            string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description)); //error1 | error2
            return (false , errorMessage);
        }   
    }


    public async Task<(bool isValid, string? Message, object? obj)> Login(UserLogin loginDTO)
    {
        var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);
        UserProfile? user = await _userManager.FindByEmailAsync(loginDTO.Email);

        if (result.IsNotAllowed)
            return (false, "You Are Not Allowed to Log in (Have You Tried to Confirm Your Email?)", null);
        
        else if (result.Succeeded)
        {
            if (user == null)
                return (false, "Your Email has not been Found!!", null);
            
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>();
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            
            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            var authenticationResponse = _jwtService.CreateJwtToken(user,authClaims);

            return (true, null, authenticationResponse);
        }
        else if (user == null)
            return (false, "Invalid email", null); 
        
        else if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            return (false, "Invalid password", null);

        return (false, null, null);
    }
        
        
    public async Task<(bool isValid, string? Message)> Logout()
    {
        await _signInManager.SignOutAsync();

        return (true, "You Have Successfully Logged Out");
    }
    
    
    public async Task<(bool isValid, string? Message, object? obj)> ConfirmEmail(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (userId == null || token == null)
            return (false, "Link expired", null);
        
        else if (user == null)
            return (false, "User not Found", null);
        
        else
        {
            token = token.Replace(" ", "+");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>();
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                
                var authenticationResponse = _jwtService.CreateJwtToken(user,authClaims);
                return (true, null, authenticationResponse);
            }
            else
                return (false, "Email not confirmed", null);
        }
    }
    
    
    public async Task<(bool isValid, string? Message)> AddDefinedAccount(int definedAccountAddNumber, ClaimsPrincipal userClaims)
    {
        var existingUser = await _userManager.GetUserAsync(userClaims);
        if (existingUser == null)
            return (false, "Try to Log-In Again,if it doesn't worked it seems you haven't signed up!!");
        
        existingUser.DefinedAccountNumbers!.Add(definedAccountAddNumber);
        
        var resultIdentity = await _userManager.UpdateAsync(existingUser);
        if (!resultIdentity.Succeeded)
            return (false, "The Defined Account ID has not been Added, Try Again");
        
        return (true, "The Account Number Has Been Successfully Added to Your List");
    }
    
    
    
    private async Task SendConfirmationEmail(UserProfile? user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"http://localhost:7160/confirm-email?userId={user.Id}&Token={token}";
        await _notifyService.SendAsync(user.Email, "Open and Confirm Your Email", $"Please confirm your account by clicking <a href='{confirmationLink}'>this link</a>;.", true);
    }
}