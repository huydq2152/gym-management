using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Users;
using GymManagement.Infrastructure.Authentication.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Infrastructure.Authentication.TokenGenerator;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Name, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("id", user.Id.ToString()),
            new("permissions", "gyms:create"),
            new("permissions", "gyms:update"),
        };
        
        // There are three types of permission-based authorization
        // 1. Resource:Operation --- read:gyms, write:gyms --- rest action : resource --- can access to the post, put, patch endpoints under the gyms resource
        // 2. Role-based --- create:admin, update:admin --- permission type : role --- admin role can access to the post and put endpoints
        // 3. Granular Permissions --- create:gym, update:gym --- action : resource --- can use the specific action on the specific resource

        AddIds(user, claims);
        AddRoles(user, claims);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void AddIds(User user, List<Claim> claims)
    {
        claims
            .AddIfValueNotNull("adminId", user.AdminId?.ToString())
            .AddIfValueNotNull("trainerId", user.TrainerId?.ToString())
            .AddIfValueNotNull("participantId", user.ParticipantId?.ToString());
    }

    private static void AddRoles(User user, List<Claim> claims)
    {
        user.GetProfileTypes().ForEach(type =>
        {
            claims.Add(new Claim("roles", type.Name));
        });
    }
}