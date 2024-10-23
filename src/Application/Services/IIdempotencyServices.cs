#pragma warning disable CS1591
namespace Application.Services;

public interface IIdempotencyService
{
    public bool ContainsKey(Guid key);

    public void AddKey(Guid key);
}