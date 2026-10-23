using static ActorWebsite.Api.Tests.TestRequests;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ActorWebsite.Api.Tests;

public sealed class AdminFlowTests
{
    private static object EventBody(string title = "Testforedrag", string? url = null, DateTimeOffset? end = null) =>
        new
        {
            title,
            description = "En beskrivelse av arrangementet.",
            startsAt = DateTimeOffset.UtcNow.AddDays(30),
            endsAt = end,
            location = "Testbiblioteket",
            address = "Testveien 1",
            organizer = "Testarrangør",
            externalUrl = url
        };

    [Fact]
    public async Task Anonymous_cannot_read_or_modify_administration()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        var id = Guid.NewGuid();
        foreach (var (method, path) in new[]
        {
            (HttpMethod.Get, "/api/admin/events/"), (HttpMethod.Get, "/api/admin/events/" + id),
            (HttpMethod.Post, "/api/admin/events/"), (HttpMethod.Put, "/api/admin/events/" + id),
            (HttpMethod.Patch, "/api/admin/events/" + id + "/publication"), (HttpMethod.Delete, "/api/admin/events/" + id)
        })
        {
            using var request = new HttpRequestMessage(method, path);
            if (method != HttpMethod.Get) request.Content = JsonContent.Create(EventBody());
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
        }
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/auth/register", new { })).StatusCode);
    }

    [Fact]
    public async Task Login_requires_csrf_and_sets_secure_cookie_logout_revokes_old_cookie()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        var body = new { email = ApiFactory.Email, password = factory.Password };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/login", body)).StatusCode);
        var login = await Login(client, factory);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var cookie = login.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("__Host-ActorWebsite.Admin="));
        Assert.Contains("httponly", cookie.ToLowerInvariant());
        Assert.Contains("secure", cookie.ToLowerInvariant());
        Assert.Contains("samesite=strict", cookie.ToLowerInvariant());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Send(client, HttpMethod.Post, "/api/auth/logout")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);

        using var replay = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false, AllowAutoRedirect = false });
        replay.DefaultRequestHeaders.Add("Cookie", cookie.Split(';')[0]);
        Assert.Equal(HttpStatusCode.Unauthorized, (await replay.GetAsync("/api/admin/events/")).StatusCode);
    }

    [Fact]
    public async Task A_user_without_administrator_role_is_denied_even_with_a_valid_identity_cookie()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync(administrator: false);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Login(client, factory)).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByEmailAsync(ApiFactory.Email))!;
        var signIn = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var principal = await signIn.CreateUserPrincipalAsync(user);
        var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
        var ticket = new AuthenticationTicket(principal,
            new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) },
            IdentityConstants.ApplicationScheme);
        using var roleless = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false, AllowAutoRedirect = false });
        roleless.DefaultRequestHeaders.Add("Cookie", "__Host-ActorWebsite.Admin=" + options.TicketDataFormat.Protect(ticket));
        Assert.Equal(HttpStatusCode.Forbidden, (await roleless.GetAsync("/api/admin/events/")).StatusCode);
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_and_login_requests_are_rate_limited()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await Login(client, factory, "incorrect password")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Login(client, factory)).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(await users.IsLockedOutAsync((await users.FindByEmailAsync(ApiFactory.Email))!));
        for (var i = 0; i < 4; i++) await Login(client, factory);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await Login(client, factory)).StatusCode);
    }

    [Fact]
    public async Task Writes_reject_missing_or_another_sessions_csrf_token()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await Login(client, factory)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/admin/events/", EventBody())).StatusCode);
        using var other = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/events/") { Content = JsonContent.Create(EventBody()) };
        request.Headers.Add("X-CSRF-TOKEN", await Csrf(other));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(request)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<ActorEvent[]>("/api/admin/events/"))!);
    }

    [Fact]
    public async Task Draft_edit_publish_unpublish_and_delete_work_against_postgresql()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await Login(client, factory)).StatusCode);
        var created = await Send(client, HttpMethod.Post, "/api/admin/events/", EventBody());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<ActorEvent>())!;
        Assert.False(item.IsPublished);
        Assert.Empty((await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!);
        var path = "/api/admin/events/" + item.Id;
        var updated = await Send(client, HttpMethod.Put, path, EventBody("Endret tittel", "https://example.org/tickets"));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Endret tittel", (await client.GetFromJsonAsync<ActorEvent>(path))!.Title);
        Assert.Equal(HttpStatusCode.OK, (await Send(client, HttpMethod.Patch, path + "/publication", new { isPublished = true })).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!);
        Assert.Equal(HttpStatusCode.OK, (await Send(client, HttpMethod.Patch, path + "/publication", new { isPublished = false })).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!);
        Assert.Equal(HttpStatusCode.NoContent, (await Send(client, HttpMethod.Delete, path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
    }

    [Theory]
    [InlineData("", null, false)]
    [InlineData("Tittel", "javascript:alert(1)", false)]
    [InlineData("Tittel", "http://example.org", false)]
    [InlineData("Tittel", null, true)]
    public async Task Invalid_input_is_rejected_without_saving(string title, string? url, bool invalidEnd)
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        await Login(client, factory);
        var response = await Send(client, HttpMethod.Post, "/api/admin/events/",
            EventBody(title, url, invalidEnd ? DateTimeOffset.UtcNow.AddDays(-1) : null));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<ActorEvent[]>("/api/admin/events/"))!);
    }

    [Fact]
    public async Task Public_views_filter_and_sort_dates_and_hide_unpublished_events()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            foreach (var (title, days, published) in new[]
            {
                ("Senere", 8, true), ("Først", 2, true), ("Utkast", 3, false),
                ("Eldst", -10, true), ("Tidligere", -1, true), ("Skjult historikk", -2, false)
            })
                db.Events.Add(new ActorEvent { Title = title, Description = "Test", Location = "Test", Address = "Test", StartsAt = DateTimeOffset.UtcNow.AddDays(days), IsPublished = published });
            await db.SaveChangesAsync();
        }
        var events = (await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!;
        Assert.Equal(new[] { "Først", "Senere" }, events.Select(item => item.Title));
        var explicitUpcoming = (await client.GetFromJsonAsync<ActorEvent[]>("/api/events?past=false"))!;
        Assert.Equal(events.Select(item => item.Id), explicitUpcoming.Select(item => item.Id));
        var past = (await client.GetFromJsonAsync<ActorEvent[]>("/api/events?past=true"))!;
        Assert.Equal(new[] { "Tidligere", "Eldst" }, past.Select(item => item.Title));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/events?past=invalid")).StatusCode);

        // Hiding applies to the archive too, and a past event can be made visible again.
        await Login(client, factory);
        var path = "/api/admin/events/" + past[0].Id + "/publication";
        Assert.Equal(HttpStatusCode.OK, (await Send(client, HttpMethod.Patch, path, new { isPublished = false })).StatusCode);
        var hidden = (await client.GetFromJsonAsync<ActorEvent[]>("/api/events?past=true"))!;
        Assert.Equal("Eldst", Assert.Single(hidden).Title);
        Assert.Equal(HttpStatusCode.OK, (await Send(client, HttpMethod.Patch, path, new { isPublished = true })).StatusCode);
        var restored = (await client.GetFromJsonAsync<ActorEvent[]>("/api/events?past=true"))!;
        Assert.Equal(past.Select(item => item.Id), restored.Select(item => item.Id));
        Assert.Equal(new[] { "Først", "Senere" }, (await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!.Select(item => item.Title));
    }

}
