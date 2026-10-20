using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaceService.Application.Database.Conventions;

/// <summary>
/// Maps a <c>Version</c> property onto Postgres's <c>xmin</c> system column.
/// </summary>
/// <remarks>
/// Every Postgres row already carries <c>xmin</c>, the id of the transaction that last wrote it,
/// so it is a concurrency token that costs no extra storage and needs no trigger. The mapping is
/// only picked up when the column is named exactly <c>xmin</c>: give it any other name and EF
/// creates a real column instead of reading the system one, which then fails on insert because
/// nothing ever writes it.
/// </remarks>
public static class ConcurrencyTokenConfiguration
{
    /// <summary>Configures the given property as an <c>xmin</c>-backed concurrency token.</summary>
    public static PropertyBuilder<uint> AsXminConcurrencyToken(this PropertyBuilder<uint> builder)
        => builder
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
}
