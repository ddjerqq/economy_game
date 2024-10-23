namespace Domain.Abstractions;

public abstract class Entity<TId>(TId id) : IEntity<TId>
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; set; } = id;

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }
}