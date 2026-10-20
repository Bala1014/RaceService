namespace RaceService.Application.Domain.Common;

/// <summary>Marks an entity that <see cref="Database.ApplicationDbContext"/> archives instead of deleting.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAtUtc { get; set; }

    Guid? DeletedByUserId { get; set; }
}
