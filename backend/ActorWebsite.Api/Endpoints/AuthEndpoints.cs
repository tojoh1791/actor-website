using ActorWebsite.Api.Contracts;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace ActorWebsite.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").AddEndpointFilter<CsrfFilter>();
        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken }));

        group.MapGet("/me", (HttpContext context) =>
            Results.Ok(new { email = context.User.Identity!.Name }))
            .RequireAuthorization(AdminSecurity.Policy);

        group.MapPost("/login", async (LoginInput input, UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn) =>
        {
            if (string.IsNullOrWhiteSpace(input.Email) || input.Email.Length > 254 ||
                string.IsNullOrEmpty(input.Password) || input.Password.Length > 1024)
                return Results.Problem(statusCode: 400, title: "Fyll inn e-post og passord.");

            var user = await users.FindByEmailAsync(input.Email.Trim());
            const string failure = "Kunne ikke logge inn. Kontroller e-post og passord, eller prøv igjen senere.";
            if (user is null)
                return Results.Problem(statusCode: 401, title: failure);

            var result = await signIn.CheckPasswordSignInAsync(user, input.Password, lockoutOnFailure: true);
            if (!result.Succeeded || !await users.IsInRoleAsync(user, AdminSecurity.Role))
                return Results.Problem(statusCode: 401, title: failure);

            await signIn.SignInAsync(user, isPersistent: input.RememberMe);
            return Results.Ok(new { email = user.Email });
        }).RequireRateLimiting("admin-login");

        group.MapPost("/logout", async (HttpContext context, UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn) =>
        {
            var user = await users.GetUserAsync(context.User);
            if (user is not null && !(await users.UpdateSecurityStampAsync(user)).Succeeded)
                return Results.Problem(statusCode: 409, title: "Kunne ikke avslutte innloggingen. Prøv igjen.");
            await signIn.SignOutAsync();
            return Results.NoContent();
        }).RequireAuthorization(AdminSecurity.Policy);
    }
}

