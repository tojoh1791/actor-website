using ActorWebsite.Api.Data;
using ActorWebsite.Api.Endpoints;
using ActorWebsite.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
if (builder.Environment.IsDevelopment())
{
    var protection = builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(
            Path.Combine(builder.Environment.ContentRootPath, ".data", "keys")));
    if (OperatingSystem.IsWindows())
    {
        protection.ProtectKeysWithDpapi();
    }
}
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/health/ready", async (ApplicationDbContext db, CancellationToken ct) =>
{
    // Query the application table so a missing migration also reports not ready.
    try
    {
        await db.Events.AsNoTracking().AnyAsync(ct);
        return Results.Ok(new { status = "ready" });
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        return Results.Problem(statusCode: 503, title: "Databasen er ikke klar.");
    }
});
app.MapEventEndpoints();
app.Run();

