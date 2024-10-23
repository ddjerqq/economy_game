using Application.Services;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;

namespace Persistence.Interceptors;

public sealed class EntitySaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        UpdateEntities(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, ct);
    }

    // ReSharper disable once CognitiveComplexity
    private static void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        var userAccessor = context.GetService<ICurrentUserAccessor>();

        var currentUserId = userAccessor.Id?.ToString();
        var dateTime = DateTime.UtcNow;

        var trackedEntityEntries = context.ChangeTracker
            .Entries()
            .Where(x => x.Entity is ITrackedEntity);

        foreach (var entry in trackedEntityEntries)
        {
            var entity = (ITrackedEntity)entry.Entity;

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                Log.Logger.Information("{UserId} {EntryState} entity with id {Entity}", currentUserId, entry.State, ((dynamic)entry.Entity).Id);

            if (entry.State == EntityState.Added)
            {
                entity.CreatedBy = currentUserId ?? "system";
                entity.Created = dateTime;
            }

            if (entry.State == EntityState.Modified || HasChangedOwnedEntities(entry))
            {
                entity.LastModifiedBy = currentUserId ?? "system";
                entity.LastModified = dateTime;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.LastModifiedBy = currentUserId ?? "system";
                entity.LastModified = dateTime;

                entity.DeletedBy = currentUserId ?? "system";
                entity.Deleted = dateTime;
            }
        }
    }

    private static bool HasChangedOwnedEntities(EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            r.TargetEntry.State is EntityState.Added or EntityState.Modified);
}