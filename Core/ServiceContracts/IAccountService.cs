using System.Security.Claims;
using Core.DTO.Auth;

namespace Core.ServiceContracts;

public interface IAccountService
{
    public Task<(bool isValid, string Message)> Register(UserRegister userRegister);
    public Task<(bool isValid, string? Message, AuthenticationResponse? obj)> Login(UserLogin loginDTO);
    // public Task<(bool isValid, string? Message)> Logout();
    public Task<(bool isValid, string? Message, AuthenticationResponse? obj)> ConfirmEmail(Guid userId, string token);
    public Task<(bool isValid, string? Message)> AddDefinedAccount(string definedAccountAddNumber, ClaimsPrincipal userClaims);
}