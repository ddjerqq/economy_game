using Application.Common;
using Application.Services;
using Domain.Aggregates;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands;

public sealed record RegisterCommand(string Email, string Username, string Password) : IRequest<(User User, string Token)>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public const string AllowedChars = @"[a-z\._]";

    public RegisterCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(c => c.Email).EmailAddress().NotEmpty();

        RuleFor(c => c.Username)
            .MinimumLength(2).MaximumLength(32)
            .Matches(AllowedChars).WithMessage("Username must be between 3 and 32 characters and contain only lowercase letters, dots, and underscores")
            .NotEmpty();

        RuleFor(c => c.Password).MinimumLength(12).MaximumLength(256).NotEmpty();

        RuleSet("async", () =>
        {
            RuleFor(c => c.Email)
                .MustAsync(async (email, ct) => await dbContext.Users.CountAsync(u => u.Email == email.ToUpperInvariant(), ct) == 0)
                .WithMessage("Email already in use");

            RuleFor(c => c.Username)
                .MustAsync(async (username, ct) => await dbContext.Users.CountAsync(u => u.Username == username.ToUpperInvariant(), ct) == 0)
                .WithMessage("Username already in use");
        });
    }
}

internal sealed class RegisterCommandHandler(IAppDbContext dbContext, IJwtGenerator jwtGenerator) : IRequestHandler<RegisterCommand, (User User, string Token)>
{
    public async Task<(User User, string Token)> Handle(RegisterCommand request, CancellationToken ct)
    {
        var user = new User(request.Username, request.Email, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        var token = jwtGenerator.GenerateToken(user.GetClaims());
        return (user, token);
    }
}