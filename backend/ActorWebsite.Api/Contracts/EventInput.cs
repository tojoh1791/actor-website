using ActorWebsite.Api.Models;
using ActorWebsite.Api.Services;

namespace ActorWebsite.Api.Contracts;

// Request validation belongs here; endpoint handlers decide authorization and persistence.
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
