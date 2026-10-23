using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

public sealed record BookingStatusInput(string? Status);
public sealed record BookingDetails(Guid Id, string ContactName, string Email, string? Phone, string? Organization,
    string EventType, DateOnly? PreferredDate, string Location, int? Participants, string Message, string Status,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static BookingDetails From(BookingRequest b) => new(b.Id, b.ContactName, b.Email, b.Phone, b.Organization,
        b.EventType, b.PreferredDate, b.Location, b.Participants, b.Message, b.Status, b.CreatedAt, b.UpdatedAt);
}

public sealed record BookingInput(Guid RequestId, string? ContactName, string? Email, string? Phone, string? Organization,
    string? EventType, DateOnly? PreferredDate, string? Location, int? Participants, string? Message,
    string? SubmissionToken, string? Website)
{
    public Dictionary<string, string[]> Validate(DateOnly today)
    {
        var errors = new Dictionary<string, string[]>();
        void Text(string key, string? value, int max, int min = 0)
        {
            if ((value?.Trim().Length ?? 0) < min) errors[key] = [$"Skriv minst {min} tegn."];
            else if (value?.Length > max) errors[key] = [$"Bruk maksimalt {max} tegn."];
            else if (value?.Any(c => char.IsControl(c) && !(key == "message" && c is '\r' or '\n' or '\t')) == true)
                errors[key] = ["Feltet inneholder ugyldige tegn."];
        }
        Text("contactName", ContactName, 100, 2);
        Text("email", Email, 254, 3);
        Text("phone", Phone, 40);
        Text("organization", Organization, 200);
        Text("location", Location, 300, 2);
        Text("message", Message, 5000, 20);
        var email = Email?.Trim();
        if (email is null || !MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
            errors["email"] = ["Skriv en gyldig e-postadresse."];
        if (!string.IsNullOrWhiteSpace(Phone) && !Regex.IsMatch(Phone, @"^[0-9+() .-]{5,40}$", RegexOptions.CultureInvariant))
            errors["phone"] = ["Skriv et gyldig telefonnummer, eller la feltet stå tomt."];
        if (EventType is not ("talk" or "book" or "acting" or "other")) errors["eventType"] = ["Velg type arrangement."];
        if (PreferredDate < today) errors["preferredDate"] = ["Velg en dato fra i dag og fremover, eller la datoen stå åpen."];
        if (Participants is < 1 or > 100000) errors["participants"] = ["Oppgi mellom 1 og 100 000 deltakere, eller la feltet stå tomt."];
        return errors;
    }

    public BookingRequest ToEntity(DateTimeOffset now)
    {
        static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        var item = new BookingRequest { Id = RequestId, ContactName = ContactName!.Trim(), Email = Email!.Trim(),
            Phone = Optional(Phone), Organization = Optional(Organization), EventType = EventType!, PreferredDate = PreferredDate,
            Location = Location!.Trim(), Participants = Participants, Message = Message!.Trim(), SubmissionHash = "", CreatedAt = now, UpdatedAt = now };
        var canonical = JsonSerializer.Serialize(new { item.ContactName, item.Email, item.Phone, item.Organization, item.EventType,
            item.PreferredDate, item.Location, item.Participants, item.Message });
        item.SubmissionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return item;
    }
}
