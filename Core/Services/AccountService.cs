using System.Security.Claims;
using Core.Domain.Entities;
using Core.Domain.ExternalServicesContracts;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.DTO.Auth;
using Core.DTO.TransactionDTO;
using Core.ServiceContracts;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Core.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<UserProfile> _userManager;
    private readonly SignInManager<UserProfile> _signInManager;
    private readonly ICurrencyAccountService _currencyAccountService;
    private readonly IAccountRepository _accountRepository;
    
    private readonly ITokenService _tokenService;
    private readonly INotificationService _notifyService;
    
    public AccountService(UserManager<UserProfile> userManager, SignInManager<UserProfile> signInManager, ITokenService tokenService,
                          INotificationService notifyService, ICurrencyAccountService currencyAccountService, IAccountRepository accountRepository)
    {
        _userManager = userManager;
        _currencyAccountService = currencyAccountService;
        _accountRepository = accountRepository;
        _signInManager = signInManager;
        
        _tokenService = tokenService;
        _notifyService = notifyService;
    }
    
    
    public async Task<Result> Register(UserRegister userRegister)
    {
        ArgumentNullException.ThrowIfNull(userRegister,$"The '{nameof(userRegister)}' object parameter is Null");

        var userReturned = await _userManager.FindByEmailAsync(userRegister.Email);
        if (userReturned != null) // if userReturned has sth, means a user with this email is already in db
            return Result.Fail("The Email is Already Registered");
        
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
            // return (true, "Please Check Your Email");
            return Result.Ok().WithSuccess("Please Check Your Email");
        }
        
        var errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description)); // error1 | error2
        return Result.Fail(errorMessage);
    }


    public async Task<Result<AuthenticationResponse>> Login(UserLogin userLogin)
    {
        ArgumentNullException.ThrowIfNull(userLogin,$"The '{nameof(userLogin)}' object parameter is Null");

        var user = await _userManager.FindByEmailAsync(userLogin.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, userLogin.Password))
            return Result.Fail("Invalid Email or Password");
        
        var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
        
        // Validations
        if (result.IsNotAllowed)
            return Result.Fail("You Are Not Allowed to Log in (Have You Tried to Confirm Your Email?");
        

        if (result.IsLockedOut)
            return Result.Fail($"You Had Too Many Attempts to Log In, You can try Again in {user.LockoutEnd}");

                
        // If Everything is Correct
        if (result.Succeeded)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            
            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            var authenticationResponse = _tokenService.CreateJwtToken(user,  userClaims.ToList());
            return Result.Ok(authenticationResponse);
        }

        return Result.Fail("Log-in Has Not Been Successful");
    }
        
        
    // public async Task<(bool isValid, string? Message)> Logout()
    // {
    //     await _signInManager.SignOutAsync();
    //
    //     return (true, "You Have Successfully Logged Out");
    // }
    
    
    public async Task<Result> ConfirmEmail(Guid userId, string token)
    {
        ArgumentNullException.ThrowIfNull(userId,$"The '{nameof(userId)}' object parameter is Null");
        ArgumentNullException.ThrowIfNull(token,$"The '{nameof(token)}' object parameter is Null");
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Fail("User Not Found");
        
        if (user.EmailConfirmed)
            return Result.Fail("Your Email Is Already Confirmed");

        
        token = token.Replace(" ", "+");
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Result.Fail("Email Has Not Been Confirmed, Link Expired (Or Token is Invalid)");

        
        return Result.Ok().WithSuccess("Email Has Been Confirmed Successfully");
    }
    
    
    public async Task<Result> AddDefinedAccount(string definedAccountAddNumber, string userName)
    {
        ArgumentNullException.ThrowIfNull(definedAccountAddNumber,$"The '{nameof(definedAccountAddNumber)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(userName,$"The '{nameof(userName)}' parameter is Null");
        
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return Result.Fail("Try to Log-In Again, if it doesn't worked it seems you haven't signed up!!");

        var (isValid, _, _) = await _currencyAccountService.GetCurrencyAccountByNumberInternal(definedAccountAddNumber);
        if (!isValid)
            return Result.Fail("The DefinedAccount Number is not in Currency Accounts");

        _accountRepository.LoadReferences(user);        
        if (user.DefinedCurrencyAccounts!.Any(acc => acc.Number == definedAccountAddNumber))
            return Result.Fail("This DefinedAccount Number is Already Added");
        
        user.DefinedAccountsJoin!.Add(new DefinedAccount()
        {
            UserProfileId = user.Id,
            CurrencyAccountNumber = definedAccountAddNumber
        });
        
        var result = await _userManager.UpdateAsync(user);
        // var userReturned = _userManager.Users.FirstOrDefault(userItem => userItem.UserName == userClaims.Identity.Name);
        if (!result.Succeeded)
            return Result.Fail("The Defined Account ID has not been Added, Try Again");
        
        return Result.Ok().WithSuccess("The Account Number Has Been Successfully Added to Your List");
    }
    
    
    public async Task<Result> SendConfirmationEmailInternal(UserProfile user)
    {
        ArgumentNullException.ThrowIfNull(user,$"The '{nameof(user)}' object parameter is Null");

        if (user.EmailConfirmed)
            return Result.Fail("Your Email Is Already Confirmed");
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if (string.IsNullOrEmpty(token))
            return Result.Fail("Send Confirmation Email Failed");
        
        var confirmationLink = $"https://localhost:7160/api/Account/confirm-email?userId={user.Id}&Token={token}";
        return await _notifyService.SendAsync(user.Email!, "Open and Confirm Your Email", $"Please confirm your account by clicking <a href='{confirmationLink}'>this link</a>;.", true);
    }    
    public async Task<Result> SendConfirmationEmail(string userName)
    {
        ArgumentNullException.ThrowIfNull(userName, $"The '{nameof(userName)}' parameter is Null");

        var user = await _userManager.FindByEmailAsync(userName);
        if (user == null)
            return Result.Fail("A User With This UserName(Email) Has Not Been Found");
        
        return await SendConfirmationEmailInternal(user);
    }
    
    
    
    // 
    public void LoadReferences(UserProfile userProfile)
    {
        _accountRepository.LoadReferences(userProfile);
    }
}