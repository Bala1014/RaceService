using System.Text;
using Microsoft.EntityFrameworkCore;

namespace RaceService.Application.Database.Conventions;

/// <summary>
/// Rewrites every table, column, key and index name to snake_case.
/// </summary>
/// <remarks>
/// Postgres folds unquoted identifiers to lower case, so a <c>StartTimeUtc</c> column can only be
/// created by quoting it — and then every hand-written query, every psql session and every
/// migration script must quote it forever, exactly as typed. Emitting <c>start_time_utc</c>
/// instead means the database is pleasant to query directly, which matters a great deal the
/// first time production misbehaves at two in the morning.
/// </remarks>
public static class SnakeCaseNamingConvention
{
    /// <summary>Applies snake_case naming across the whole model.</summary>
    public static void ApplySnakeCaseNames(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName is not null)
            {
                entity.SetTableName(tableName.ToSnakeCase());
            }

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
            }
        }
    }

    /// <summary>Converts a PascalCase or camelCase identifier to snake_case.</summary>
    public static string ToSnakeCase(this string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var current = name[i];

            if (char.IsUpper(current))
            {
                // Break before an upper-case letter, but not at the start and not in the middle
                // of an acronym: "SeasonID" becomes season_id, not season_i_d.
                var previousIsLowerOrDigit = i > 0 && (char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1]));
                var nextIsLower = i + 1 < name.Length && char.IsLower(name[i + 1]);
                var startsNewWord = i > 0 && (previousIsLowerOrDigit || (nextIsLower && char.IsUpper(name[i - 1])));

                if (startsNewWord && builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
            }
            else if (current == '.' || current == ' ' || current == '-')
            {
                if (builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }
            }
            else
            {
                builder.Append(current);
            }
        }

        return builder.ToString();
    }
}
