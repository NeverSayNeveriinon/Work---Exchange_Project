using System.Security.Claims;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;

namespace Core.ServiceContracts;

public interface IAccountService
{
    public Task<(bool isValid, string Message)> Register(UserRegister? userRegister);
    public Task<(bool isValid, string? Message, AuthenticationResponse? obj)> Login(UserLogin? userLogin);
    public Task<(bool isValid, string? Message)> ConfirmEmail(Guid? userId, string? token);
    public Task<(bool isValid, string? Message)> AddDefinedAccount(string definedAccountAddNumber, string userName);
    public Task<(bool isValid, string message)> SendConfirmationEmailInternal(UserProfile? user);
    public Task<(bool isValid, string message)> SendConfirmationEmail(string userName);
    // public Task<(bool isValid, string? Message)> Logout();
}