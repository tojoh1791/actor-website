using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Services;
using ImageMagick;

namespace ActorWebsite.Api.Tests;

public sealed class EventImageTests
{
    private static async Task<string> Csrf(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf")).GetProperty("token").GetString()!;

    private static async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string path, object body)
    {
        using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", await Csrf(client));
        return await client.SendAsync(request);
    }

    private static async Task Login(HttpClient client, ApiFactory factory) =>
        Assert.Equal(HttpStatusCode.OK, (await Send(client, HttpMethod.Post, "/api/auth/login",
            new { email = ApiFactory.Email, password = factory.Password })).StatusCode);

    private static async Task<HttpResponseMessage> Upload(HttpClient client, byte[] bytes, bool csrf = true)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, EventImageEndpointsPath)
        { Content = new ByteArrayContent(bytes) };
        // Deliberately misleading: validation must inspect the bytes, not this header.
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        if (csrf) request.Headers.Add("X-CSRF-TOKEN", await Csrf(client));
        return await client.SendAsync(request);
    }
    private const string EventImageEndpointsPath = "/api/admin/event-images";
    private static object EventBody(string? imageUrl) => new
    {
        title = "Et arrangement med bilde", description = "Beskrivelse", location = "Biblioteket", address = "Testveien 1",
        startsAt = DateTimeOffset.UtcNow.AddDays(30), imageUrl
    };

    [Fact]
    public async Task Upload_requires_an_administrator_and_csrf()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, (await Upload(client, [1, 2, 3], false)).StatusCode);
        await Login(client, factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await Upload(client, [1, 2, 3], false)).StatusCode);
    }

    [Theory]
    [InlineData(MagickFormat.Jpeg)]
    [InlineData(MagickFormat.Png)]
    [InlineData(MagickFormat.WebP)]
    public async Task Supported_images_are_reencoded_and_publicly_readable(MagickFormat format)
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        await Login(client, factory);
        using var image = new MagickImage(MagickColors.Green, 100, 80);
        var uploaded = await Upload(client, image.ToByteArray(format));
        Assert.Equal(HttpStatusCode.Created, uploaded.StatusCode);
        var url = (await uploaded.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("imageUrl").GetString()!;
        Assert.StartsWith(EventImageStore.UrlPrefix, url);
        using var anonymous = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var read = await anonymous.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal("image/jpeg", read.Content.Headers.ContentType!.MediaType);
        Assert.Equal("nosniff", read.Headers.GetValues("X-Content-Type-Options").Single());
        using var output = new MagickImage(await read.Content.ReadAsByteArrayAsync());
        Assert.Equal(MagickFormat.Jpeg, output.Format);
        Assert.Equal(100u, output.Width);
        Assert.Equal(80u, output.Height);
    }

    [Fact]
    public async Task Stored_image_is_resized_metadata_removed_and_event_can_return_to_default()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        await Login(client, factory);
        using var image = new MagickImage(MagickColors.Green, 3000, 1500);
        var exif = new ExifProfile();
        exif.SetValue(ExifTag.ImageDescription, "Private image metadata");
        image.SetProfile(exif);
        var uploaded = await Upload(client, image.ToByteArray(MagickFormat.Jpeg));
        Assert.Equal(HttpStatusCode.Created, uploaded.StatusCode);
        var url = (await uploaded.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("imageUrl").GetString()!;
        using var output = new MagickImage(await client.GetByteArrayAsync(url));
        Assert.Equal(1600u, output.Width);
        Assert.Equal(800u, output.Height);
        Assert.Null(output.GetExifProfile());
        Assert.Null(output.GetIptcProfile());
        Assert.Null(output.GetXmpProfile());
        var created = await Send(client, HttpMethod.Post, "/api/admin/events/", EventBody(url));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<ActorEvent>())!;
        Assert.Equal(url, item.ImageUrl);
        var path = "/api/admin/events/" + item.Id;
        Assert.Equal(url, (await client.GetFromJsonAsync<ActorEvent>(path))!.ImageUrl);
        await Send(client, HttpMethod.Patch, path + "/publication", new { isPublished = true });
        Assert.Equal(url, (await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!.Single().ImageUrl);
        var updated = await Send(client, HttpMethod.Put, path, EventBody(null));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Null((await updated.Content.ReadFromJsonAsync<ActorEvent>())!.ImageUrl);
        Assert.Null((await client.GetFromJsonAsync<ActorEvent[]>("/api/events"))!.Single().ImageUrl);
    }

    [Fact]
    public async Task Invalid_files_oversized_files_and_untrusted_image_references_are_rejected()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        await Login(client, factory);
        foreach (var bytes in new[] { "<script>alert(1)</script>"u8.ToArray(), "<svg xmlns='http://www.w3.org/2000/svg'/>"u8.ToArray(),
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0 } })
            Assert.Equal(HttpStatusCode.BadRequest, (await Upload(client, bytes)).StatusCode);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await Upload(client, new byte[EventImageStore.MaxBytes + 1])).StatusCode);
        foreach (var url in new[] { "https://example.org/photo.jpg", "/api/event-images/../../appsettings.json",
            "/api/event-images/" + Guid.NewGuid().ToString("N") + ".jpg" })
            Assert.Equal(HttpStatusCode.BadRequest, (await Send(client, HttpMethod.Post, "/api/admin/events/", EventBody(url))).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<ActorEvent[]>("/api/admin/events/"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/event-images/not-an-image.jpg")).StatusCode);
    }
}
