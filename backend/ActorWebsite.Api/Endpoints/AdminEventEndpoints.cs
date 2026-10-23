using ActorWebsite.Api.Contracts;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Security;
using ActorWebsite.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Endpoints;

public static class AdminEventEndpoints
{
    public static void MapAdminEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/events")
            .RequireAuthorization(AdminSecurity.Policy)
            .AddEndpointFilter<CsrfFilter>();

        group.MapGet("/", async (ApplicationDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Events.AsNoTracking().OrderBy(e => e.StartsAt).ToListAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db, CancellationToken ct) =>
            await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct) is { } item
                ? Results.Ok(item) : Results.NotFound());

        group.MapPost("/", async (EventInput input, ApplicationDbContext db, EventImageStore images, CancellationToken ct) =>
        {
            var errors = input.Validate(images);
            if (errors.Count != 0) return Results.ValidationProblem(errors);
            var item = new ActorEvent { Title = "", Description = "", Location = "", Address = "" };
            input.Apply(item);
            // Creation always starts as a draft. Publication is an explicit action.
            db.Events.Add(item);
            await db.SaveChangesAsync(ct);
            return Results.Created("/api/admin/events/" + item.Id, item);
        });

        group.MapPut("/{id:guid}", async (Guid id, EventInput input, ApplicationDbContext db, EventImageStore images, CancellationToken ct) =>
        {
            var errors = input.Validate(images);
            if (errors.Count != 0) return Results.ValidationProblem(errors);
            var item = await db.Events.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            input.Apply(item);
            item.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(item);
        });

        group.MapPatch("/{id:guid}/publication", async (Guid id, PublicationInput input,
            ApplicationDbContext db, EventImageStore images, CancellationToken ct) =>
        {
            var item = await db.Events.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            if (input.IsPublished)
            {
                var errors = EventInput.From(item).Validate(images);
                if (errors.Count != 0) return Results.ValidationProblem(errors);
            }
            item.IsPublished = input.IsPublished;
            item.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(item);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Events.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            db.Events.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
