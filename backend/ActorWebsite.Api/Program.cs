using ActorWebsite.Api.Data;
using ActorWebsite.Api.Endpoints;
using ActorWebsite.Api.Security;
using ActorWebsite.Api.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var resetPassword = args.Contains("--reset-admin-password");
var accountCommand = resetPassword || args.Contains("--create-admin");

// Configure storage and security before building the request pipeline.
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
builder.Services.AddAdminSecurity(builder.Environment);
builder.Services.AddSingleton<EventImageStore>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<BookingFormTokens>();
EventImageStore.ConfigureLimits();
// JSON requests here are small; reject oversized submissions before model binding.
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);

if (accountCommand)
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var app = builder.Build();
if (accountCommand)
{
    Environment.ExitCode = await AdminAccountCommand.RunAsync(app.Services, resetPassword);
    return;
}
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    EventImageEndpoints.SetImageUploadLimit(context);
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    if (context.Request.Path.StartsWithSegments("/api/auth") || context.Request.Path.StartsWithSegments("/api/admin") || context.Request.Path.StartsWithSegments("/api/bookings"))
        context.Response.Headers.CacheControl = "no-store";
    await next();
});
// Authenticate before authorization and rate limiting inspect the request.
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

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
        await db.BookingRequests.AsNoTracking().AnyAsync(ct);
        return Results.Ok(new { status = "ready" });
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        return Results.Problem(statusCode: 503, title: "Databasen er ikke klar.");
    }
});
app.MapEventEndpoints();
app.MapAuthEndpoints();
app.MapAdminEventEndpoints();
app.MapEventImageEndpoints();
app.MapBookingEndpoints();
app.Run();


// Allows integration tests to boot the real application pipeline.
public partial class Program;
