using MediatR;

namespace Application.Economy.Commands;

internal sealed record TransactionCommand(Ulid SenderId, Ulid ReceiverId, decimal Amount) : IRequest<bool>;
