using System.Security.Claims;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;
using FluentResults;

namespace Core.ServiceContracts;

public interface IAccountService
{
    Task<Result> Register(UserRegister userRegister);
    Task<Result<AuthenticationResponse>> Login(UserLogin userLogin);
    Task<Result> ConfirmEmail(Guid userId, string token);
    Task<Result> AddDefinedAccount(string definedAccountAddNumber, string userName);
    
    Task<Result> SendConfirmationEmailInternal(UserProfile user);
    Task<Result> SendConfirmationEmail(string userName);

    void LoadReferences(UserProfile userProfile);
    // Task<Result> Logout();
}