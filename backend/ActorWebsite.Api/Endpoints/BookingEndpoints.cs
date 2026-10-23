using ActorWebsite.Api.Contracts;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ActorWebsite.Api.Endpoints;

public static class BookingEndpoints
{
    public static readonly string[] Statuses = ["new", "inProgress", "agreed", "declined"];

    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/bookings").AddEndpointFilter<CsrfFilter>();
        publicGroup.MapGet("/form", (HttpContext context, IAntiforgery antiforgery, BookingFormTokens tokens) =>
            Results.Ok(new { csrfToken = antiforgery.GetAndStoreTokens(context).RequestToken, submissionToken = tokens.Create() }))
            .RequireRateLimiting("booking-form");
        publicGroup.MapPost("/", async (BookingInput input, BookingFormTokens tokens, TimeProvider clock,
            ApplicationDbContext db, CancellationToken ct) =>
        {
            if (!tokens.IsValid(input.SubmissionToken))
                return Results.Problem(statusCode: 400, title: "Skjemaet er utløpt eller ble sendt for raskt. Last inn skjemaet på nytt og prøv igjen.");
            if (input.RequestId == Guid.Empty)
                return Results.Problem(statusCode: 400, title: "Last inn skjemaet på nytt og prøv igjen.");
            // Silently discard the honeypot without creating an inbox item.
            if (!string.IsNullOrWhiteSpace(input.Website)) return Receipt(input.RequestId);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(),
                TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo")).DateTime);
            var errors = input.Validate(today);
            if (errors.Count != 0) return Results.ValidationProblem(errors);
            var item = input.ToEntity(clock.GetUtcNow());
            var existing = await db.BookingRequests.AsNoTracking().SingleOrDefaultAsync(b => b.Id == item.Id, ct);
            if (existing is not null) return RetryReceipt(existing, item);
            db.BookingRequests.Add(item);
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Concurrent retries must still create exactly one inbox item.
                db.Entry(item).State = EntityState.Detached;
                existing = await db.BookingRequests.AsNoTracking().SingleAsync(b => b.Id == item.Id, ct);
                return RetryReceipt(existing, item);
            }
            return Receipt(item.Id);
        }).RequireRateLimiting("booking-submit");

        var admin = app.MapGroup("/api/admin/bookings").RequireAuthorization(AdminSecurity.Policy).AddEndpointFilter<CsrfFilter>();
        admin.MapGet("/summary", async (ApplicationDbContext db, CancellationToken ct) =>
            Results.Ok(new { newCount = await db.BookingRequests.CountAsync(b => b.Status == "new", ct) }));
        admin.MapGet("/", async (string? status, int? page, ApplicationDbContext db, CancellationToken ct) =>
        {
            if ((status is not null && status != "all" && !Statuses.Contains(status)) || page is < 1 or > 100000)
                return Results.Problem(statusCode: 400, title: "Velg en gyldig visning.");
            var query = db.BookingRequests.AsNoTracking();
            if (status is not null && status != "all") query = query.Where(b => b.Status == status);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.Id)
                .Skip(((page ?? 1) - 1) * 20).Take(20)
                .Select(b => new { b.Id, b.ContactName, b.Organization, b.EventType, b.PreferredDate, b.Location, b.Status, b.CreatedAt }).ToListAsync(ct);
            return Results.Ok(new { items, total, page = page ?? 1, pageSize = 20 });
        });
        admin.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db, CancellationToken ct) =>
            await db.BookingRequests.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, ct) is { } item
                ? Results.Ok(BookingDetails.From(item)) : Results.NotFound());
        admin.MapGet("/{id:guid}/event-draft", async (Guid id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var item = await db.BookingRequests.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, ct);
            if (item is null) return Results.NotFound();
            if (item.EventType is not ("talk" or "book" or "other"))
                return Results.Problem(statusCode: 409, title: "Skuespilleroppdrag kan ikke gjøres om til arrangementer.");
            if (item.Status != "agreed")
                return Results.Problem(statusCode: 409, title: "Lagre forespørselen som «Avtalt» før du oppretter et arrangement.");
            // Only copy fields intended for review as public event information.
            // The private message and personal contact details stay in the inbox.
            var (title, description) = item.EventType switch
            {
                "talk" => ("Foredrag med Arben Bala", "Et foredrag med Arben Bala om tilhørighet, valg og veier videre."),
                "book" => ("Boksamtale med Arben Bala", "En samtale med Arben Bala om Balkanbrødre og historien bak boken."),
                _ => ("Arrangement med Arben Bala", "")
            };
            return Results.Ok(new BookingEventDraft(title, description, item.PreferredDate, item.Location, item.Organization));
        });
        admin.MapPatch("/{id:guid}/status", async (Guid id, BookingStatusInput input, ApplicationDbContext db, TimeProvider clock, CancellationToken ct) =>
        {
            if (input.Status is null || !Statuses.Contains(input.Status))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["status"] = ["Velg en gyldig status."] });
            var item = await db.BookingRequests.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            item.Status = input.Status;
            item.UpdatedAt = clock.GetUtcNow();
            await db.SaveChangesAsync(ct);
            return Results.Ok(BookingDetails.From(item));
        });
        admin.MapDelete("/{id:guid}", async (Guid id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var item = await db.BookingRequests.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            db.BookingRequests.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static IResult Receipt(Guid id) => Results.Accepted(value: new { reference = "AB-" + id.ToString("N")[..8].ToUpperInvariant() });
    private static IResult RetryReceipt(BookingRequest existing, BookingRequest incoming) => existing.SubmissionHash == incoming.SubmissionHash
        ? Receipt(existing.Id)
        : Results.Problem(statusCode: 409, title: "En annen forespørsel er allerede mottatt med denne referansen. Last inn skjemaet på nytt før du sender en ny.");
}
