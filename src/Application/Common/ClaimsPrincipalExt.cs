using System.Security.Claims;
using Domain.Aggregates;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application.Common;

public static class ClaimsPrincipalExt
{
    private static string? GetClaimValue(this ClaimsPrincipal principal, string claimType) => principal
        .Claims
        .FirstOrDefault(x => x.Type == claimType)?
        .Value;

    public static Ulid? GetId(this ClaimsPrincipal principal) =>
        Ulid.TryParse(principal.GetClaimValue(JwtRegisteredClaimNames.Sid), out var id) ? id : null;

    public static string? GetFullName(this ClaimsPrincipal principal) =>
        principal.GetClaimValue(JwtRegisteredClaimNames.Name);

    public static string? GetEmail(this ClaimsPrincipal principal) =>
        principal.GetClaimValue(JwtRegisteredClaimNames.Email);

    public static IEnumerable<Claim> GetClaims(this User user)
    {
        return
        [
            new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        ];
    }
}