using Domain.Abstractions;

namespace Domain.Aggregates;

public sealed class User(Ulid id) : AggregateRoot<Ulid>(id)
{
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string PasswordHash { get; set; } = default!;
    public decimal Balance { get; set; }

    public bool TryTransfer(User other, decimal amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        other.Balance += amount;
        return true;
    }

    /// <summary>
    /// Constructor for new users
    /// </summary>
    public User(string username, string email, string password) : this(Ulid.NewUlid())
    {
        Username = username.ToUpperInvariant();
        Email = email.ToUpperInvariant();
        PasswordHash = BC.EnhancedHashPassword(password);
        Balance = 0;

        Created = DateTime.UtcNow;
        CreatedBy = "system";
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public User(User user) : this(user.Id)
    {
        Username = user.Username;
        Email = user.Email;
        PasswordHash = user.PasswordHash;
        Balance = user.Balance;

        Created = user.Created;
        CreatedBy = user.CreatedBy;

        LastModified = user.LastModified;
        LastModifiedBy = user.LastModifiedBy;

        Deleted = user.Deleted;
        DeletedBy = user.DeletedBy;
    }
}