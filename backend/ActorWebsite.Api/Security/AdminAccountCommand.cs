using System.Net.Mail;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Security;

public static class AdminAccountCommand
{
    public static async Task<int> RunAsync(IServiceProvider services, bool reset)
    {
        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Kjør kommandoen i en interaktiv terminal. Passord skal ikke gis som argument eller miljøvariabel.");
            return 1;
        }
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            Console.Error.WriteLine("Kjør npm run db:migrate før du oppretter administrator.");
            return 1;
        }
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Console.Write("Administratorens e-post: ");
        var email = Console.ReadLine()?.Trim() ?? "";
        if (!MailAddress.TryCreate(email, out var parsed) || parsed.Address != email || email.Length > 254)
        {
            Console.Error.WriteLine("Oppgi en gyldig e-postadresse.");
            return 1;
        }
        var user = await users.FindByEmailAsync(email);
        if (reset && (user is null || !await users.IsInRoleAsync(user, AdminSecurity.Role)))
        {
            Console.Error.WriteLine("Fant ingen administrator med denne adressen.");
            return 1;
        }
        if (!reset && (user is not null || (await users.GetUsersInRoleAsync(AdminSecurity.Role)).Count != 0))
        {
            Console.Error.WriteLine("En konto finnes allerede. Bruk admin:reset-password ved glemt passord.");
            return 1;
        }
        Console.WriteLine("Bruk minst 14 tegn. En lang passfrase er tillatt.");
        Console.Write("Nytt passord: ");
        var password = ReadPassword();
        Console.Write("Gjenta passordet: ");
        var confirmation = ReadPassword();
        if (password != confirmation)
        {
            Console.Error.WriteLine("Passordene er forskjellige. Ingen endring ble lagret.");
            return 1;
        }
        await using var transaction = await db.Database.BeginTransactionAsync();
        IdentityResult result;
        if (reset)
        {
            var token = await users.GeneratePasswordResetTokenAsync(user!);
            result = await users.ResetPasswordAsync(user!, token, password);
            if (result.Succeeded)
            {
                await users.SetLockoutEndDateAsync(user!, null);
                await users.ResetAccessFailedCountAsync(user!);
            }
        }
        else
        {
            if (!await roles.RoleExistsAsync(AdminSecurity.Role))
            {
                result = await roles.CreateAsync(new IdentityRole(AdminSecurity.Role));
                if (!result.Succeeded) return Fail(result);
            }
            user = new ApplicationUser { UserName = email, Email = email };
            result = await users.CreateAsync(user, password);
            if (result.Succeeded) result = await users.AddToRoleAsync(user, AdminSecurity.Role);
        }
        if (!result.Succeeded) return Fail(result);
        await transaction.CommitAsync();
        Console.WriteLine(reset ? "Passordet er endret. Tidligere innlogginger er ugyldige." : "Administratoren er opprettet. Åpne /admin for å logge inn.");
        return 0;
    }

    private static int Fail(IdentityResult result)
    {
        foreach (var error in result.Errors) Console.Error.WriteLine(error.Description);
        return 1;
    }

    private static string ReadPassword()
    {
        var value = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return value.ToString(); }
            if (key.Key == ConsoleKey.Backspace) { if (value.Length > 0) value.Length--; }
            else if (!char.IsControl(key.KeyChar) && value.Length < 1024) value.Append(key.KeyChar);
        }
    }
}
