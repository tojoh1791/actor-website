namespace ActorWebsite.Api.Models;

public sealed class BookingRequest
{
    public Guid Id { get; set; }
    public required string ContactName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Organization { get; set; }
    public required string EventType { get; set; }
    public DateOnly? PreferredDate { get; set; }
    public required string Location { get; set; }
    public int? Participants { get; set; }
    public required string Message { get; set; }
    public string Status { get; set; } = "new";
    public required string SubmissionHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
