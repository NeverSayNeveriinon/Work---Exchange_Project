using Core.Domain.IdentityEntities;
using Core.DTO.Auth;

namespace Core.ServiceContracts;

public interface IJwtService
{
    public AuthenticationResponse CreateJwtToken(UserProfile user);
}