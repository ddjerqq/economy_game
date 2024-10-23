using System.Security.Claims;
using Application.Common;
using Application.Services;
using Domain.Aggregates;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor, IAppDbContext dbContext) : ICurrentUserAccessor
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Ulid? Id => User?.GetId();
    public string? FullName => User?.GetFullName();
    public string? Email => User?.GetEmail();

    public async Task<User?> TryGetCurrentUserAsync(CancellationToken ct = default)
    {
        if (Id is not { } id)
            return null;

        return await dbContext.Users.FindAsync([id], ct)
               ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {id} not found");
    }
}