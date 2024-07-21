using System.Security.Claims;
using Core.Domain.Entities;
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
    private readonly SignInManager<UserProfile> _signInManager;
    private readonly ICurrencyAccountService _currencyAccountService;
    
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notifyService;
    
    public AccountService(UserManager<UserProfile> userManager, SignInManager<UserProfile> signInManager, IJwtService jwtService,
                          INotificationService notifyService, ICurrencyAccountService currencyAccountService)
    {
        _userManager = userManager;
        _currencyAccountService = currencyAccountService;
        _signInManager = signInManager;
        
        _jwtService = jwtService;
        _notifyService = notifyService;
    }
    
    
    public async Task<(bool isValid, string Message)> Register(UserRegister? userRegister)
    {
        ArgumentNullException.ThrowIfNull(userRegister,"The 'userRegister' object parameter is Null");

        var userReturned = await _userManager.FindByEmailAsync(userRegister.Email);
        if (userReturned != null) // if userReturned has sth, means a user with this email is already in db
            return (false, "The Email is Already Registered");
        
        var user = new UserProfile()
        {
            UserName = userRegister.Email,
            Email = userRegister.Email,
            PersonName = userRegister.PersonName,
            PhoneNumber = userRegister.Phone
        };
        
        var result = await _userManager.CreateAsync(user, userRegister.Password);
        
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, userRegister.Role);
            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, userRegister.Role));
            _ = SendConfirmationEmailInternal(user);
            // var (isValid, message) = SendConfirmationEmail(user).Result;
            // var (isValid, message) = Task.Run(() => SendConfirmationEmail(user));
            // if (!isValid)
            //     return (false, message);
            
            // return (true, message);
            return (true, "Please Check Your Email");
        }
        
        // if (!await _roleManager.RoleExistsAsync(userRegister.Role))
        // {
        //     UserRole userRole = new UserRole() { Name = userRegister.Role };
        //     await _roleManager.CreateAsync(userRole);
        //     await _roleManager.AddClaimAsync(userRole,new Claim(ClaimTypes.Role, userRole.Name));
        // }
        var errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description)); // error1 | error2
        return (false , errorMessage);
    }


    public async Task<(bool isValid, string? Message, AuthenticationResponse? obj)> Login(UserLogin? userLogin)
    {
        ArgumentNullException.ThrowIfNull(userLogin,"The 'userLogin' object parameter is Null");

        var user = await _userManager.FindByEmailAsync(userLogin.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, userLogin.Password))
            return (false, "Invalid Email or Password", null);
        
        var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
        
        // Validations
        if (result.IsNotAllowed)
            return (false, "You Are Not Allowed to Log in (Have You Tried to Confirm Your Email?)", null);

        if (result.IsLockedOut)
            return (false, $"You Had Too Many Attempts to Log In, You can try Again in {user.LockoutEnd}", null);
                
        // If Everything is Correct
        if (result.Succeeded)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            // var authClaims = new List<Claim>();
            // foreach (var userRole in userRoles)
            // {
            //     authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            // }
            
            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            var authenticationResponse = _jwtService.CreateJwtToken(user,userClaims.ToList());
            return (true, null, authenticationResponse);
        }

        return (false, "Log-in Has Not Been Successful", null);
    }
        
        
    // public async Task<(bool isValid, string? Message)> Logout()
    // {
    //     await _signInManager.SignOutAsync();
    //
    //     return (true, "You Have Successfully Logged Out");
    // }
    
    
    public async Task<(bool isValid, string? Message)> ConfirmEmail(Guid? userId, string? token)
    {
        ArgumentNullException.ThrowIfNull(userId,"The 'userId' object parameter is Null");
        ArgumentNullException.ThrowIfNull(token,"The 'token' object parameter is Null");
        
        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return (false, "User Not Found");
        
        if (user.EmailConfirmed)
            return (true, "Your Email Is Already Confirmed");
        
        token = token.Replace(" ", "+");
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return (false, "Email Has Not Been Confirmed, Link Expired (Or Token is Invalid)");
        
        return (true, "Email Has Been Confirmed Successfully");
    }
    
    
    public async Task<(bool isValid, string? Message)> AddDefinedAccount(string definedAccountAddNumber, string userName)
    {
        ArgumentNullException.ThrowIfNull(definedAccountAddNumber,"The 'definedAccountAddNumber' parameter is Null");
        ArgumentNullException.ThrowIfNull(userName,"The 'userName' parameter is Null");
        
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return (false, "Try to Log-In Again, if it doesn't worked it seems you haven't signed up!!");

        var (isValid, _, _) = _currencyAccountService.GetCurrencyAccountByNumberInternal(definedAccountAddNumber);
        if (!isValid)
            return (false, "The DefinedAccount Number is not in Currency Accounts");

        user.DefinedAccountsJoin!.Add(new DefinedAccount()
        {
            UserProfileId = user.Id,
            CurrencyAccountNumber = definedAccountAddNumber
        });
        
        var result = await _userManager.UpdateAsync(user);
        // var userReturned = _userManager.Users.FirstOrDefault(userItem => userItem.UserName == userClaims.Identity.Name);
        if (!result.Succeeded)
            return (false, "The Defined Account ID has not been Added, Try Again");
        
        return (true, "The Account Number Has Been Successfully Added to Your List");
    }
    
    
    public async Task<(bool isValid, string message)> SendConfirmationEmailInternal(UserProfile? user)
    {
        ArgumentNullException.ThrowIfNull(user,"The 'user' object parameter is Null");

        if (user.EmailConfirmed)
            return (true, "Your Email Is Already Confirmed");
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if (string.IsNullOrEmpty(token))
            return (false, "Send Confirmation Email Failed");
        
        var confirmationLink = $"https://localhost:7160/api/Account/confirm-email?userId={user.Id}&Token={token}";
        return await _notifyService.SendAsync(user.Email!, "Open and Confirm Your Email", $"Please confirm your account by clicking <a href='{confirmationLink}'>this link</a>;.", true);
    }    
    public async Task<(bool isValid, string message)> SendConfirmationEmail(string userName)
    {
        ArgumentNullException.ThrowIfNull(userName,"The 'userName' parameter is Null");

        var user = await _userManager.FindByEmailAsync(userName);
        if (user == null)
            return (false, "A User With This UserName(Email) Has Not Been Found");
        
        return await SendConfirmationEmailInternal(user);
    }
}