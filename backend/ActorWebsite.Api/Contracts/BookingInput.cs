using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ActorWebsite.Api.Models;

namespace ActorWebsite.Api.Contracts;

public sealed record BookingStatusInput(string? Status);
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
        var item = new BookingRequest
        {
            Id = RequestId,
            ContactName = ContactName!.Trim(),
            Email = Email!.Trim(),
            Phone = Optional(Phone),
            Organization = Optional(Organization),
            EventType = EventType!,
            PreferredDate = PreferredDate,
            Location = Location!.Trim(),
            Participants = Participants,
            Message = Message!.Trim(),
            SubmissionHash = "",
            CreatedAt = now,
            UpdatedAt = now
        };
        var canonical = JsonSerializer.Serialize(new
        {
            item.ContactName,
            item.Email,
            item.Phone,
            item.Organization,
            item.EventType,
            item.PreferredDate,
            item.Location,
            item.Participants,
            item.Message
        });
        item.SubmissionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return item;
    }
}
