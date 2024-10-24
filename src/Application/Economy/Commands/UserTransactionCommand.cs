using MediatR;

namespace Application.Economy.Commands;

public sealed record UserTransactionCommand(Ulid ReceiverId, decimal Amount) : IRequest<bool>;