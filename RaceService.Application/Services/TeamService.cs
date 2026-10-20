using Microsoft.EntityFrameworkCore;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Mapping;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlugService _slugs;

    /// <summary>Initialises the service.</summary>
    public TeamService(ApplicationDbContext context, ISlugService slugs)
    {
        _context = context;
        _slugs = slugs;
    }

    /// <inheritdoc />
    public async Task<PagedResult<TeamSummaryResponse>> SearchAsync(
        TeamQuery query,
        CancellationToken cancellationToken = default)
    {
        var teams = _context.Teams.AsQueryable();

        if (query.IsActive is not null)
        {
            teams = teams.Where(team => team.IsActive == query.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            teams = teams.Where(team => team.CountryCode == query.CountryCode);
        }

        if (query.SeasonId is not null)
        {
            teams = teams.Where(team => team.Entries.Any(entry => entry.SeasonId == query.SeasonId));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            teams = teams.Where(team =>
                EF.Functions.ILike(team.Name, term) || EF.Functions.ILike(team.ShortName, term));
        }

        var total = await teams.LongCountAsync(cancellationToken);

        var page = await teams
            .OrderBy(team => team.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(team => new TeamSummaryResponse
            {
                Id = team.Id,
                Name = team.Name,
                ShortName = team.ShortName,
                Slug = team.Slug,
                CountryCode = team.CountryCode,
                LogoUrl = team.LogoUrl,
                PrimaryColorHex = team.PrimaryColorHex,
                IsActive = team.IsActive
            })
            .ToListAsync(cancellationToken);

        return PagedResult<TeamSummaryResponse>.Create(page, query.Page, query.PageSize, total);
    }

    /// <inheritdoc />
    public Task<Result<TeamDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync(team => team.Id == id, $"id '{id}'", cancellationToken);

    /// <inheritdoc />
    public Task<Result<TeamDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => GetAsync(team => team.Slug == slug, $"slug '{slug}'", cancellationToken);

    /// <inheritdoc />
    public async Task<Result<TeamDetailResponse>> CreateAsync(
        CreateTeamRequest request,
        CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _slugs.GenerateUniqueAsync(
                request.Name,
                candidate => _context.Teams.AnyAsync(existing => existing.Slug == candidate, cancellationToken))
            : _slugs.Generate(request.Slug);

        if (await _context.Teams.AnyAsync(existing => existing.Slug == slug, cancellationToken))
        {
            return Result.Failure<TeamDetailResponse>(
                Error.Conflict("Team.DuplicateSlug", $"A team with slug '{slug}' already exists."));
        }

        var team = new Team
        {
            Name = request.Name,
            OfficialName = request.OfficialName,
            ShortName = request.ShortName,
            Slug = slug,
            CountryCode = request.CountryCode?.ToUpperInvariant(),
            BaseCity = request.BaseCity,
            BaseCountry = request.BaseCountry,
            FoundedYear = request.FoundedYear,
            LogoUrl = request.LogoUrl,
            PrimaryColorHex = request.PrimaryColorHex,
            WebsiteUrl = request.WebsiteUrl,
            Description = request.Description
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(team.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<TeamDetailResponse>> UpdateAsync(
        Guid id,
        UpdateTeamRequest request,
        CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (team is null)
        {
            return Result.Failure<TeamDetailResponse>(Error.NotFound("Team.NotFound", $"No team with id '{id}'."));
        }

        team.Name = request.Name;
        team.OfficialName = request.OfficialName;
        team.ShortName = request.ShortName;
        team.CountryCode = request.CountryCode?.ToUpperInvariant();
        team.BaseCity = request.BaseCity;
        team.BaseCountry = request.BaseCountry;
        team.FoundedYear = request.FoundedYear;
        team.LogoUrl = request.LogoUrl;
        team.PrimaryColorHex = request.PrimaryColorHex;
        team.WebsiteUrl = request.WebsiteUrl;
        team.Description = request.Description;
        team.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (team is null)
        {
            return Result.Failure(Error.NotFound("Team.NotFound", $"No team with id '{id}'."));
        }

        if (await _context.Entries.AnyAsync(entry => entry.TeamId == id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Team.HasEntries",
                "A team with season entries cannot be archived. Set it inactive instead."));
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<TeamDetailResponse>> GetAsync(
        System.Linq.Expressions.Expression<Func<Team, bool>> predicate,
        string describedBy,
        CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .Include(candidate => candidate.Entries).ThenInclude(entry => entry.Season).ThenInclude(season => season.Series)
            .Include(candidate => candidate.Entries).ThenInclude(entry => entry.VehicleClass)
            .Include(candidate => candidate.Entries).ThenInclude(entry => entry.ChassisManufacturer)
            .Include(candidate => candidate.Entries).ThenInclude(entry => entry.EngineManufacturer)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (team is null)
        {
            return Result.Failure<TeamDetailResponse>(Error.NotFound("Team.NotFound", $"No team with {describedBy}."));
        }

        return Result.Success(new TeamDetailResponse
        {
            Id = team.Id,
            Name = team.Name,
            OfficialName = team.OfficialName,
            ShortName = team.ShortName,
            Slug = team.Slug,
            CountryCode = team.CountryCode,
            BaseCity = team.BaseCity,
            BaseCountry = team.BaseCountry,
            FoundedYear = team.FoundedYear,
            LogoUrl = team.LogoUrl,
            PrimaryColorHex = team.PrimaryColorHex,
            WebsiteUrl = team.WebsiteUrl,
            Description = team.Description,
            IsActive = team.IsActive,
            Entries = [.. team.Entries
                .OrderByDescending(entry => entry.Season.Year)
                .ThenBy(entry => entry.CarNumber)
                .Select(entry =>
                {
                    entry.Team = team;
                    return entry.ToResponse();
                })]
        });
    }
}
