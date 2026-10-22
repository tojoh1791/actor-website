namespace ActorWebsite.Api.Models;

public sealed class ActorEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    // PostgreSQL timestamptz: always persist UTC values.
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public required string Location { get; set; }
    public required string Address { get; set; }
    public string? Organizer { get; set; }
    public string? ExternalUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
