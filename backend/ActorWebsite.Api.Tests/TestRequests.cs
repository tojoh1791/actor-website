using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ActorWebsite.Api.Tests;

// Exercise the real cookie/CSRF flow in every suite instead of bypassing authentication.
internal static class TestRequests
{
    public static async Task<string> Csrf(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf")).GetProperty("token").GetString()!;

    public static async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", await Csrf(client));
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> Login(HttpClient client, ApiFactory factory, string? password = null) =>
        Send(client, HttpMethod.Post, "/api/auth/login", new { email = ApiFactory.Email, password = password ?? factory.Password });

    public static async Task SignIn(HttpClient client, ApiFactory factory) =>
        Assert.Equal(HttpStatusCode.OK, (await Login(client, factory)).StatusCode);
}
