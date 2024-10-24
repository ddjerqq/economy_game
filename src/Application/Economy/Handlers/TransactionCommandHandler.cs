using Application.Economy.Commands;
using Application.Services;
using MediatR;

namespace Application.Economy.Handlers;

internal sealed class TransactionCommandHandler(IAppDbContext dbContext, ICurrentUserAccessor currentUser)
    : IRequestHandler<TransactionCommand, bool>,
        IRequestHandler<UserTransactionCommand, bool>
{
    public async Task<bool> Handle(TransactionCommand request, CancellationToken ct)
    {
        var sender = await dbContext.Users.FindAsync([request.SenderId], ct);
        var receiver = await dbContext.Users.FindAsync([request.ReceiverId], ct);

        if (sender is null || receiver is null) return false;
        if (!sender.TryTransfer(receiver, request.Amount)) return false;

        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> Handle(UserTransactionCommand request, CancellationToken ct)
    {
        var sender = await currentUser.GetCurrentUserAsync(ct);
        if (sender.Balance < request.Amount) return false;
        var command = new TransactionCommand(sender.Id, request.ReceiverId, request.Amount);
        return await Handle(command, ct);
    }
}