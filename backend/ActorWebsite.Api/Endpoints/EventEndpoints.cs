using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var now = DateTimeOffset.UtcNow;
            var events = await db.Events.AsNoTracking()
                .Where(e => e.IsPublished && e.StartsAt >= now)
                .OrderBy(e => e.StartsAt)
                .Select(e => new PublicEvent(e.Id, e.Title, e.Description, e.StartsAt,
                    e.EndsAt, e.Location, e.Address, e.Organizer, e.ExternalUrl, e.ImageUrl))
                .ToListAsync(ct);
            return Results.Ok(events);
        }).WithName("GetUpcomingEvents");
    }
}

public sealed record PublicEvent(Guid Id, string Title, string Description,
    DateTimeOffset StartsAt, DateTimeOffset? EndsAt, string Location, string Address,
    string? Organizer, string? ExternalUrl, string? ImageUrl);
