using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events", async (bool? past, ApplicationDbContext db, CancellationToken ct) =>
        {
            var now = DateTimeOffset.UtcNow;
            // Publication is required in both views; hidden events never enter the public archive.
            var query = db.Events.AsNoTracking()
                .Where(e => e.IsPublished && (past == true ? e.StartsAt < now : e.StartsAt >= now));
            var ordered = past == true
                ? query.OrderByDescending(e => e.StartsAt)
                : query.OrderBy(e => e.StartsAt);
            var events = await ordered
                .Select(e => new PublicEvent(e.Id, e.Title, e.Description, e.StartsAt,
                    e.EndsAt, e.Location, e.Address, e.Organizer, e.ExternalUrl, e.ImageUrl))
                .ToListAsync(ct);
            return Results.Ok(events);
        }).WithName("GetPublicEvents");
    }
}

public sealed record PublicEvent(Guid Id, string Title, string Description,
    DateTimeOffset StartsAt, DateTimeOffset? EndsAt, string Location, string Address,
    string? Organizer, string? ExternalUrl, string? ImageUrl);
