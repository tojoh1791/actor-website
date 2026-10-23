using ActorWebsite.Api.Models;

namespace ActorWebsite.Api.Contracts;

// Explicit response types keep private fields such as SubmissionHash out of the API.
public sealed record BookingEventDraft(string Title, string Description, DateOnly? PreferredDate, string Location, string? Organizer);
public sealed record BookingDetails(Guid Id, string ContactName, string Email, string? Phone, string? Organization,
    string EventType, DateOnly? PreferredDate, string Location, int? Participants, string Message, string Status,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static BookingDetails From(BookingRequest b) => new(b.Id, b.ContactName, b.Email, b.Phone, b.Organization,
        b.EventType, b.PreferredDate, b.Location, b.Participants, b.Message, b.Status, b.CreatedAt, b.UpdatedAt);
}
