using System.Security.Claims;

namespace Application.Services;

public interface IJwtGenerator
{
    public string GenerateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null);
}