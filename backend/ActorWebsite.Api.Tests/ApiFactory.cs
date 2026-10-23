using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ActorWebsite.Api.Tests;

// No test runs against the developer's application database.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = "actor_tests_" + Guid.NewGuid().ToString("N");
    private readonly string adminConnection;
    private readonly string testConnection;
    private bool cleaned;
    private readonly bool development;
    private readonly TimeProvider clock;
    private readonly string imageDirectory = Path.Combine(Path.GetTempPath(), "actor-images-tests-" + Guid.NewGuid().ToString("N"));
    public string Password { get; } = Guid.NewGuid().ToString("N") + "!aA";
    public const string Email = "admin@example.test";

    public ApiFactory(bool development = false, TimeProvider? clock = null)
    {
        this.development = development;
        this.clock = clock ?? TimeProvider.System;
        // WebApplicationFactory's default discovery searches for a .sln file;
        // this repository uses .slnx and keeps the API below backend/.
        Environment.SetEnvironmentVariable("ASPNETCORE_TEST_CONTENTROOT_ACTORWEBSITE_API",
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile())!, "../ActorWebsite.Api")));
        var source = Environment.GetEnvironmentVariable("ACTOR_TEST_POSTGRES")
            ?? throw new InvalidOperationException("Kjør npm run test:api med en kjørende lokal PostgreSQL.");
        var admin = new NpgsqlConnectionStringBuilder(source) { Database = "postgres", Pooling = false };
        adminConnection = admin.ConnectionString;
        using var connection = new NpgsqlConnection(adminConnection);
        connection.Open();
        using var command = new NpgsqlCommand("CREATE DATABASE \"" + databaseName + "\"", connection);
        command.ExecuteNonQuery();
        var test = new NpgsqlConnectionStringBuilder(source) { Database = databaseName, Pooling = true, MaxPoolSize = 5 };
        testConnection = test.ConnectionString;
    }

    private static string SourceFile([System.Runtime.CompilerServices.CallerFilePath] string file = "") => file;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(development ? "Development" : "Testing");
        builder.UseSetting("EventImages:Directory", imageDirectory);
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(testConnection));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(clock);
        });
    }

    public async Task<HttpClient> ReadyClientAsync(bool administrator = true)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True((await roles.CreateAsync(new IdentityRole(AdminSecurity.Role))).Succeeded);
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = Email, Email = Email };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        if (administrator) Assert.True((await users.AddToRoleAsync(user, AdminSecurity.Role)).Succeeded);
        return client;
    }

    public override async ValueTask DisposeAsync()
    {
        if (cleaned) return;
        cleaned = true;
        await base.DisposeAsync();
        var imageRoot = Path.GetFullPath(imageDirectory);
        if (!imageRoot.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)
            || !Path.GetFileName(imageRoot).StartsWith("actor-images-tests-", StringComparison.Ordinal))
            throw new InvalidOperationException("Ugyldig testmappe.");
        if (Directory.Exists(imageRoot)) Directory.Delete(imageRoot, recursive: true);
        // Name is generated by this fixture and never taken from configuration.
        if (!databaseName.StartsWith("actor_tests_") || databaseName.Length != 44)
            throw new InvalidOperationException("Ugyldig testdatabasenavn.");
        using var connection = new NpgsqlConnection(adminConnection);
        await connection.OpenAsync();
        using var command = new NpgsqlCommand("DROP DATABASE IF EXISTS \"" + databaseName + "\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
        GC.SuppressFinalize(this);
    }

}
