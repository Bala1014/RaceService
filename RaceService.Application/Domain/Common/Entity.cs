namespace RaceService.Application.Domain.Common;

/// <summary>
/// Base for every entity keyed by a single client-generated id.
/// </summary>
/// <remarks>
/// Ids are UUID v7: sortable by creation time, unlike UUID v4, which scatters inserts across the
/// whole primary key index instead of appending to it.
/// </remarks>
public abstract class Entity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

/// <summary>Base for an entity whose audit columns are stamped automatically on save.</summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}
