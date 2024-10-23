using System.Linq.Expressions;
using System.Reflection;
using Application.Common;
using Application.Services;
using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Persistence.Interceptors;
using Persistence.ValueConverters;

namespace Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    EntitySaveChangesInterceptor entitySaveChangesInterceptor,
    ConvertDomainEventsToOutboxMessagesInterceptor convertDomainEventsToOutboxMessagesInterceptor)
    : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.Load(nameof(Persistence)));

        base.OnModelCreating(builder);

        ConfigureSoftDeletion(builder);
        SnakeCaseRename(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(convertDomainEventsToOutboxMessagesInterceptor, entitySaveChangesInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<DateTimeToUtcDateTimeValueConverter>();
        builder.Properties<Ulid>().HaveConversion<UlidToStringValueConverter>();

        base.ConfigureConventions(builder);
    }

    private static void ConfigureSoftDeletion(ModelBuilder builder)
    {
        var entities = builder.Model
            .GetEntityTypes()
            .Where(entity => typeof(ITrackedEntity).IsAssignableFrom(entity.ClrType))
            // A filter may only be applied to the root entity type
            .Where(entity => entity.GetRootType().ClrType == entity.ClrType);

        foreach (var entity in entities)
        {
            var parameter = Expression.Parameter(entity.ClrType);
            var deletedProperty = typeof(ITrackedEntity).GetProperty(nameof(ITrackedEntity.Deleted))!;

            // entity => entity.Deleted == null
            var lambda = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(parameter, deletedProperty),
                    Expression.Constant(null)),
                parameter);

            builder.Entity(entity.ClrType)
                .HasQueryFilter(lambda);
        }
    }

    private static void SnakeCaseRename(ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var entityTableName = entity.GetTableName()!
                .Replace("AspNet", string.Empty)
                .TrimEnd('s')
                .ToSnakeCase();

            entity.SetTableName(entityTableName);

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()!.ToSnakeCase());

            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName()!.ToSnakeCase());

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()!.ToSnakeCase());
        }
    }
}