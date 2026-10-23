using ActorWebsite.Api.Security;
using ActorWebsite.Api.Services;
using Microsoft.AspNetCore.Http.Features;

namespace ActorWebsite.Api.Endpoints;

public static class EventImageEndpoints
{
    public const string UploadPath = "/api/admin/event-images";

    public static void MapEventImageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(EventImageStore.UrlPrefix + "{name}", (string name, EventImageStore images) =>
        {
            var path = images.Find(EventImageStore.UrlPrefix + name);
            return path is null ? Results.NotFound() : Results.File(path, "image/jpeg");
        });

        app.MapPost(UploadPath, async (HttpContext context, EventImageStore images, CancellationToken ct) =>
        {
            if (context.Request.ContentLength > EventImageStore.MaxBytes)
                return Results.Problem(statusCode: 413, title: "Bildet må være mindre enn 8 MB.");
            try
            {
                var imageUrl = await images.SaveAsync(context.Request.Body, ct);
                return Results.Created(imageUrl, new { imageUrl });
            }
            catch (ImageUploadException ex)
            {
                return Results.Problem(statusCode: ex.Status, title: ex.Message);
            }
            catch (BadHttpRequestException ex) when (ex.StatusCode == 413)
            {
                return Results.Problem(statusCode: 413, title: "Bildet må være mindre enn 8 MB.");
            }
        }).RequireAuthorization(AdminSecurity.Policy).AddEndpointFilter<CsrfFilter>();
    }

    public static void SetImageUploadLimit(HttpContext context)
    {
        // Keep the smaller global limit for every JSON endpoint.
        if (context.Request.Path == UploadPath && HttpMethods.IsPost(context.Request.Method))
        {
            var feature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (feature is { IsReadOnly: false }) feature.MaxRequestBodySize = EventImageStore.MaxBytes;
        }
    }
}
