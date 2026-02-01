using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymTracker.Api.Auth;

public class AuthContext(IHttpContextAccessor httpContextAccessor) : IAuthContext
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available");

    public Guid UserId
    {
        get
        {
            var subClaim = _httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (subClaim == null)
                throw new InvalidOperationException("Subject (sub) claim not found in JWT");

            return !Guid.TryParse(subClaim.Value, out var userId) 
                ? throw new InvalidOperationException("Subject claim is not a valid GUID") 
                : userId;
        }
    }

    public string Username
    {
        get
        {
            var claim = _httpContext.User.FindFirst(JwtRegisteredClaimNames.UniqueName);
            
            return claim?.Value ?? throw new InvalidOperationException("Username claim not found in JWT");
        }
    }
}
