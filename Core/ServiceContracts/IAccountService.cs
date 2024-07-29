using System.Security.Claims;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;

namespace Core.ServiceContracts;

public interface IAccountService
{
    Task<(bool isValid, string Message)> Register(UserRegister userRegister);
    Task<(bool isValid, string? Message, AuthenticationResponse? obj)> Login(UserLogin userLogin);
    Task<(bool isValid, string? Message)> ConfirmEmail(Guid userId, string token);
    Task<(bool isValid, string? Message)> AddDefinedAccount(string definedAccountAddNumber, string userName);
    
    Task<(bool isValid, string message)> SendConfirmationEmailInternal(UserProfile user);
    Task<(bool isValid, string message)> SendConfirmationEmail(string userName);
    // Task<(bool isValid, string? Message)> Logout();
}