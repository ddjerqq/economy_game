#pragma warning disable CS1591
namespace WebApi.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireIdempotencyAttribute : Attribute;