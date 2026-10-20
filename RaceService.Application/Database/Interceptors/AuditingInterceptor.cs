using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RaceService.Application.Abstractions;
using RaceService.Application.Domain.Common;

namespace RaceService.Application.Database.Interceptors;

/// <summary>
/// Stamps audit columns and turns deletes of soft-deletable entities into updates.
/// </summary>
/// <remarks>
/// Doing this in an interceptor rather than in each service is what makes the guarantee actually
/// hold: there is no code path that saves an entity and forgets. It also means a soft-deletable
/// row cannot be hard deleted by accident — <c>Remove()</c> is rewritten into a flag update
/// before the SQL is generated.
/// </remarks>
public sealed class AuditingInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    /// <summary>Initialises the interceptor.</summary>
    public AuditingInterceptor(ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _currentUser = currentUser;
        _clock = clock;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = _clock.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAtUtc = now;
                        auditable.UpdatedAtUtc = now;
                        auditable.CreatedByUserId = userId;
                        auditable.UpdatedByUserId = userId;
                        break;

                    case EntityState.Modified:
                        auditable.UpdatedAtUtc = now;
                        auditable.UpdatedByUserId = userId;
                        // Never let an update rewrite who created the row.
                        entry.Property(nameof(AuditableEntity.CreatedAtUtc)).IsModified = false;
                        entry.Property(nameof(AuditableEntity.CreatedByUserId)).IsModified = false;
                        break;
                }
            }

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAtUtc = now;
                softDeletable.DeletedByUserId = userId;
            }
        }
    }
}
