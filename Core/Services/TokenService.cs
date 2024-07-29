using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Domain.IdentityEntities;
using Core.DTO.Auth;
using Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Core.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    /// <summary>
    /// Generates a JWT token using the given user's information and the configuration settings.
    /// </summary>
    /// <param name="user">UserProfile object</param>
    /// <param name="claims">User Claims</param>
    /// <returns>AuthenticationResponse that includes token</returns>
    public AuthenticationResponse CreateJwtToken(UserProfile user, List<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(user,$"The '{nameof(user)}' parameter is Null");
        ArgumentNullException.ThrowIfNull(claims,$"The '{nameof(claims)}' parameter is Null");

        var jwt_EXPIRATION_MINUTES = _configuration["Jwt:EXPIRATION_MINUTES"];
        var jwt_key = _configuration["Jwt:Key"];
        var jwt_Issuer = _configuration["Jwt:Issuer"];
        var jwt_Audience = _configuration["Jwt:Audience"];
        
        if ( new List<string?>(){jwt_EXPIRATION_MINUTES,jwt_key,jwt_Issuer,jwt_Audience}.Exists(string.IsNullOrEmpty) )
            throw new InvalidOperationException("Check Jwt Settings (EXPIRATION_MINUTES,Key,Issuer,Audience) In Your Configuration");
        
        
        var expiration = DateTime.Now.AddMinutes(Convert.ToDouble(jwt_EXPIRATION_MINUTES));
 
        claims.AddRange(
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //Subject (user id)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JWT unique ID
            new Claim(ClaimTypes.Name, user.Email), //Unique name of the user (Email)
            new Claim(ClaimTypes.Email, user.Email), //Unique email of the user (Email)
            // new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), //Issued at (date and time of token generation)
        ]);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_key!));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenGenerator = new JwtSecurityToken
        (
            jwt_Issuer,
            jwt_Audience,
            claims,
            expires: expiration,
            signingCredentials: signingCredentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        string token = tokenHandler.WriteToken(tokenGenerator);

        // Create and return an AuthenticationResponse object containing the token, user email and token expiration time.
        return new AuthenticationResponse() { Token = token, Email = user.Email, Expiration = expiration };
    }
}