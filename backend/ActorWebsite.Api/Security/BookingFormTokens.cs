using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace ActorWebsite.Api.Security;

public sealed class BookingFormTokens(IDataProtectionProvider provider, TimeProvider clock)
{
    private readonly IDataProtector protector = provider.CreateProtector("ActorWebsite.BookingForm.v1");
    public string Create() => protector.Protect(clock.GetUtcNow().ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture));

    public bool IsValid(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 2048) return false;
        try
        {
            if (!long.TryParse(protector.Unprotect(token), out var issued)) return false;
            var age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeMilliseconds(issued);
            return age >= TimeSpan.FromSeconds(2) && age <= TimeSpan.FromHours(2);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or ArgumentOutOfRangeException)
        { return false; }
    }
}
