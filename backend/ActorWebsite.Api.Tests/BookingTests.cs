using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ActorWebsite.Api.Tests;

public sealed class BookingTests
{
    private sealed class Clock : TimeProvider
    {
        private DateTimeOffset now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan time) => now += time;
    }
    private static async Task<JsonElement> Form(HttpClient client) => await client.GetFromJsonAsync<JsonElement>("/api/bookings/form");
    private static object Body(JsonElement form, Guid? requestId = null, string? email = null, string? website = null,
        DateOnly? date = null, string? eventType = null, string? token = null) => new
    {
        requestId = requestId ?? Guid.NewGuid(), contactName = "Testarrangør", email = email ?? "organizer@example.test", phone = "+47 12345678",
        organization = "Testbiblioteket", eventType = eventType ?? "talk", preferredDate = date, location = "Oslo", participants = 120,
        message = "Vi ønsker et foredrag for voksne deltakere på biblioteket.", submissionToken = token ?? form.GetProperty("submissionToken").GetString(), website,
        status = "agreed", isPublished = true // Untrusted extra properties must not control administration.
    };
    private static async Task<HttpResponseMessage> Submit(HttpClient client, JsonElement form, object body, bool csrf = true)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/bookings/") { Content = JsonContent.Create(body) };
        if (csrf) request.Headers.Add("X-CSRF-TOKEN", form.GetProperty("csrfToken").GetString());
        return await client.SendAsync(request);
    }
    private static async Task<HttpResponseMessage> AdminWrite(HttpClient client, HttpMethod method, string path, object? body = null)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("token").GetString());
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }
    private static async Task Login(HttpClient client, ApiFactory factory) => Assert.Equal(HttpStatusCode.OK,
        (await AdminWrite(client, HttpMethod.Post, "/api/auth/login", new { email = ApiFactory.Email, password = factory.Password })).StatusCode);

    [Fact]
    public async Task Anonymous_submission_is_private_persistent_idempotent_and_never_creates_an_event()
    {
        var clock = new Clock();
        await using var factory = new ApiFactory(clock: clock);
        using var client = await factory.ReadyClientAsync();
        var form = await Form(client);
        clock.Advance(TimeSpan.FromSeconds(3));
        var id = Guid.NewGuid();
        var body = Body(form, id);
        var sent = await Submit(client, form, body);
        Assert.Equal(HttpStatusCode.Accepted, sent.StatusCode);
        Assert.Equal("no-store", sent.Headers.CacheControl!.ToString());
        var receipt = await sent.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(receipt.EnumerateObject());
        Assert.StartsWith("AB-", receipt.GetProperty("reference").GetString());
        Assert.Equal(HttpStatusCode.Accepted, (await Submit(client, form, body)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Submit(client, form, Body(form, id, email: "different@example.test"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/bookings/" + id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/bookings/" + id)).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = Assert.Single(await db.BookingRequests.ToListAsync());
        Assert.Equal("new", stored.Status);
        Assert.Equal("Testarrangør", stored.ContactName);
        Assert.Null(stored.PreferredDate);
        Assert.Empty(await db.Events.ToListAsync());
    }

    [Fact]
    public async Task Admin_can_filter_read_change_status_and_delete_but_anonymous_users_cannot()
    {
        var clock = new Clock();
        await using var factory = new ApiFactory(clock: clock);
        using var client = await factory.ReadyClientAsync();
        var form = await Form(client);
        clock.Advance(TimeSpan.FromSeconds(3));
        var id = Guid.NewGuid();
        await Submit(client, form, Body(form, id));
        var path = "/api/admin/bookings/" + id;
        foreach (var url in new[] { "/api/admin/bookings/", "/api/admin/bookings/summary", path, path + "/event-draft" })
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PatchAsJsonAsync(path + "/status", new { status = "agreed" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync(path)).StatusCode);
        await Login(client, factory);
        var summary = await client.GetFromJsonAsync<JsonElement>("/api/admin/bookings/summary");
        Assert.Equal(1, summary.GetProperty("newCount").GetInt32());
        var list = await client.GetFromJsonAsync<JsonElement>("/api/admin/bookings/?status=new");
        Assert.Equal(1, list.GetProperty("total").GetInt32());
        Assert.False(list.GetProperty("items")[0].TryGetProperty("message", out _));
        Assert.False(list.GetProperty("items")[0].TryGetProperty("submissionHash", out _));
        var details = await client.GetFromJsonAsync<JsonElement>(path);
        Assert.Equal("organizer@example.test", details.GetProperty("email").GetString());
        Assert.False(details.TryGetProperty("submissionHash", out _));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PatchAsJsonAsync(path + "/status", new { status = "agreed" })).StatusCode);
        foreach (var status in new[] { "inProgress", "agreed", "declined", "new" })
        {
            var response = await AdminWrite(client, HttpMethod.Patch, path + "/status", new { status });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(status, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());
        }
        Assert.Equal(HttpStatusCode.BadRequest, (await AdminWrite(client, HttpMethod.Patch, path + "/status", new { status = "published" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/admin/bookings/?status=secret&page=0")).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await AdminWrite(client, HttpMethod.Delete, path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task Roleless_user_cannot_read_or_modify_bookings()
    {
        // Reuse the real Identity authorization setup: a valid admin session loses
        // its role, which is rechecked by security-stamp validation on every request.
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        await Login(client, factory);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            Assert.True((await users.RemoveFromRoleAsync((await users.FindByEmailAsync(ApiFactory.Email))!, "Administrator")).Succeeded);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/bookings/")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/bookings/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/bookings/" + Guid.NewGuid() + "/event-draft")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AdminWrite(client, HttpMethod.Patch, "/api/admin/bookings/" + Guid.NewGuid() + "/status", new { status = "agreed" })).StatusCode);
    }

    [Fact]
    public async Task Event_prefill_uses_public_fields_only_and_does_not_create_or_publish_anything()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        var date = new DateOnly(2027, 6, 18);
        var bookings = new[] { "talk", "book", "other" }.Select(type => new BookingRequest
        {
            Id = Guid.NewGuid(), ContactName = "Privat kontaktperson", Email = "private@example.test", Phone = "+47 98765432",
            Organization = type == "other" ? null : "Kulturhuset", EventType = type, PreferredDate = type == "other" ? null : date,
            Location = "Oslo", Message = "Privat melding med opplysninger som ikke skal publiseres.", Status = "agreed", SubmissionHash = new string('a', 64)
        }).ToArray();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.BookingRequests.AddRange(bookings);
            await db.SaveChangesAsync();
        }
        await Login(client, factory);
        foreach (var booking in bookings)
        {
            var response = await client.GetAsync("/api/admin/bookings/" + booking.Id + "/event-draft");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.CacheControl!.NoStore);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(new[] { "description", "location", "organizer", "preferredDate", "title" }, json.EnumerateObject().Select(p => p.Name).Order().ToArray());
            Assert.Equal("Oslo", json.GetProperty("location").GetString());
            Assert.Equal(booking.Organization, json.GetProperty("organizer").GetString());
            Assert.Equal(booking.PreferredDate?.ToString("yyyy-MM-dd"), json.GetProperty("preferredDate").GetString());
            Assert.Contains("Arben Bala", json.GetProperty("title").GetString());
            var publicText = string.Join(" ", json.EnumerateObject().Select(p => p.Value.GetString()));
            Assert.DoesNotContain(booking.ContactName, publicText);
            Assert.DoesNotContain(booking.Email, publicText);
            Assert.DoesNotContain(booking.Phone!, publicText);
            Assert.DoesNotContain(booking.Message, publicText);
        }
        // A previously valid link must check the current status again on reload.
        var path = "/api/admin/bookings/" + bookings[0].Id;
        Assert.Equal(HttpStatusCode.OK, (await AdminWrite(client, HttpMethod.Patch, path + "/status", new { status = "declined" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.GetAsync(path + "/event-draft")).StatusCode);
        await using var check = factory.Services.CreateAsyncScope();
        Assert.Empty(await check.ServiceProvider.GetRequiredService<ApplicationDbContext>().Events.ToListAsync());
    }

    [Fact]
    public async Task Acting_and_unconfirmed_bookings_cannot_be_used_to_prefill_events_even_with_a_direct_link()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        var cases = new[] { ("acting", "agreed"), ("acting", "new"), ("talk", "new"), ("talk", "inProgress"), ("book", "declined"), ("other", "new"), ("unknown", "agreed") };
        var bookings = cases.Select(value => new BookingRequest
        {
            Id = Guid.NewGuid(), ContactName = "Testperson", Email = "test@example.test", EventType = value.Item1,
            Status = value.Item2, Location = "Oslo", Message = "En privat testforespørsel.", SubmissionHash = new string('b', 64)
        }).ToArray();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.BookingRequests.AddRange(bookings);
            await db.SaveChangesAsync();
        }
        await Login(client, factory);
        foreach (var booking in bookings)
            Assert.Equal(HttpStatusCode.Conflict, (await client.GetAsync("/api/admin/bookings/" + booking.Id + "/event-draft")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/admin/bookings/" + Guid.NewGuid() + "/event-draft")).StatusCode);
        await using var check = factory.Services.CreateAsyncScope();
        Assert.Empty(await check.ServiceProvider.GetRequiredService<ApplicationDbContext>().Events.ToListAsync());
    }

    [Fact]
    public async Task Spam_trap_csrf_and_form_age_are_enforced_without_storing_spam()
    {
        var clock = new Clock();
        await using var factory = new ApiFactory(clock: clock);
        using var client = await factory.ReadyClientAsync();
        var form = await Form(client);
        Assert.Equal(HttpStatusCode.BadRequest, (await Submit(client, form, Body(form), csrf: false)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Submit(client, form, Body(form))).StatusCode);
        clock.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal(HttpStatusCode.Accepted, (await Submit(client, form, Body(form, website: "spam.example"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Submit(client, form, Body(form, token: "forged"))).StatusCode);
        clock.Advance(TimeSpan.FromHours(3));
        Assert.Equal(HttpStatusCode.BadRequest, (await Submit(client, form, Body(form))).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().BookingRequests.ToListAsync());
    }

    [Fact]
    public async Task Submissions_are_limited_per_ip()
    {
        var clock = new Clock();
        await using var factory = new ApiFactory(clock: clock);
        using var client = await factory.ReadyClientAsync();
        var form = await Form(client);
        clock.Advance(TimeSpan.FromSeconds(3));
        for (var i = 0; i < 5; i++) Assert.Equal(HttpStatusCode.Accepted, (await Submit(client, form, Body(form))).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await Submit(client, form, Body(form))).StatusCode);
    }

    [Theory]
    [InlineData("not-an-email", "talk", false)]
    [InlineData("ok@example.test", "unknown", false)]
    [InlineData("ok@example.test", "talk", true)]
    public async Task Invalid_fields_are_rejected(string email, string eventType, bool pastDate)
    {
        var clock = new Clock();
        await using var factory = new ApiFactory(clock: clock);
        using var client = await factory.ReadyClientAsync();
        var form = await Form(client);
        clock.Advance(TimeSpan.FromSeconds(3));
        var response = await Submit(client, form, Body(form, email: email, eventType: eventType,
            date: pastDate ? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2) : null));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.NotEmpty(errors.EnumerateObject());
        await using var scope = factory.Services.CreateAsyncScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().BookingRequests.ToListAsync());
    }
}
