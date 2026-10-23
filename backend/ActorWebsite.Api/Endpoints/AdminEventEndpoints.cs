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
                if (item.StartsAt < DateTimeOffset.UtcNow)
                    errors["startsAt"] = ["Et arrangement som har startet kan ikke publiseres som kommende."];
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

public sealed record PublicationInput(bool IsPublished);

public sealed record EventInput(string? Title, string? Description, DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt, string? Location, string? Address, string? Organizer, string? ExternalUrl, string? ImageUrl = null)
{
    public Dictionary<string, string[]> Validate(EventImageStore images)
    {
        var errors = new Dictionary<string, string[]>();
        void Text(string key, string? value, int max, bool required = false)
        {
            if (required && string.IsNullOrWhiteSpace(value)) errors[key] = ["Dette feltet må fylles ut."];
            else if (value?.Length > max) errors[key] = [$"Bruk maksimalt {max} tegn."];
        }
        Text("title", Title, 200, true);
        Text("description", Description, 10000, true);
        Text("location", Location, 200, true);
        Text("address", Address, 500, true);
        Text("organizer", Organizer, 200);
        Text("externalUrl", ExternalUrl, 2048);
        if (ImageUrl is not null && images.Find(ImageUrl) is null)
            errors["imageUrl"] = ["Velg et bilde på nytt, eller bruk standardbildet."];
        if (StartsAt is null || StartsAt < new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero))
            errors["startsAt"] = ["Velg en gyldig dato og et klokkeslett."];
        if (EndsAt is not null && EndsAt <= StartsAt)
            errors["endsAt"] = ["Slutt må være etter start."];
        if (!string.IsNullOrWhiteSpace(ExternalUrl) &&
            (!Uri.TryCreate(ExternalUrl.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)))
            errors["externalUrl"] = ["Lenken må være en fullstendig HTTPS-adresse."];
        return errors;
    }

    public void Apply(ActorEvent item)
    {
        item.Title = Title!.Trim();
        item.Description = Description!.Trim();
        item.StartsAt = StartsAt!.Value.ToUniversalTime();
        item.EndsAt = EndsAt?.ToUniversalTime();
        item.Location = Location!.Trim();
        item.Address = Address!.Trim();
        item.Organizer = string.IsNullOrWhiteSpace(Organizer) ? null : Organizer.Trim();
        item.ExternalUrl = string.IsNullOrWhiteSpace(ExternalUrl) ? null : ExternalUrl.Trim();
        item.ImageUrl = ImageUrl;
    }

    public static EventInput From(ActorEvent item) => new(item.Title, item.Description, item.StartsAt,
        item.EndsAt, item.Location, item.Address, item.Organizer, item.ExternalUrl, item.ImageUrl);
}
