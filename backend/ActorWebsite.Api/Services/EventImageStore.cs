using ImageMagick;

namespace ActorWebsite.Api.Services;

public sealed class EventImageStore(IHostEnvironment environment, IConfiguration configuration)
{
    public const int MaxBytes = 8 * 1024 * 1024;
    public const string UrlPrefix = "/api/event-images/";
    // Configure a persistent directory/volume in production and include it in backups.
    private readonly string directory = Path.GetFullPath(configuration["EventImages:Directory"]
        ?? Path.Combine(environment.ContentRootPath, ".data", "event-images"));
    private readonly SemaphoreSlim processing = new(1, 1);

    public static void ConfigureLimits()
    {
        ResourceLimits.Memory = 256 * 1024 * 1024;
        ResourceLimits.Disk = 0;
        ResourceLimits.Thread = 2;
        ResourceLimits.Width = 20000;
        ResourceLimits.Height = 20000;
        ResourceLimits.MaxProfileSize = 1024 * 1024;
    }

    public string? Find(string? url)
    {
        if (url is null || !url.StartsWith(UrlPrefix, StringComparison.Ordinal)) return null;
        var name = url[UrlPrefix.Length..];
        if (name.Length != 36 || !name.EndsWith(".jpg", StringComparison.Ordinal)
            || !Guid.TryParseExact(name[..32], "N", out _)) return null;
        var path = Path.Combine(directory, name);
        return File.Exists(path) ? path : null;
    }

    public async Task<string> SaveAsync(Stream input, CancellationToken ct)
    {
        // Bound memory even when Content-Length is missing or misleading.
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int count;
        while ((count = await input.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + count > MaxBytes) throw new ImageUploadException("Bildet må være mindre enn 8 MB.", 413);
            await buffer.WriteAsync(chunk.AsMemory(0, count), ct);
        }
        await processing.WaitAsync(ct);
        try
        {
            // Choose a decoder from the actual file signature, never a client filename
            // or MIME type. Force that decoder so other formats/delegates cannot run.
            var bytes = buffer.GetBuffer();
            var format = buffer.Length >= 12 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff ? MagickFormat.Jpeg
                : buffer.Length >= 12 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10}) ? MagickFormat.Png
                : buffer.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8) ? MagickFormat.WebP
                : throw new ImageUploadException("Velg et JPG-, PNG- eller WebP-bilde.");
            var options = new MagickReadSettings { Format = format, FrameCount = 1 };
            buffer.Position = 0;
            var info = new MagickImageInfo(buffer, options);
            if ((long)info.Width * info.Height > 40_000_000)
                throw new ImageUploadException("Bildet er for stort. Velg et bilde med høyst 40 megapiksler.");
            buffer.Position = 0;
            using var image = new MagickImage(buffer, options);
            image.AutoOrient();
            image.Resize(new MagickGeometry(1600, 1600) { Greater = true });
            image.BackgroundColor = MagickColors.White;
            image.Alpha(AlphaAction.Remove);
            // Re-encode pixels only. In particular, do not publish GPS/EXIF metadata.
            image.Strip();
            image.Quality = 85;
            Directory.CreateDirectory(directory);
            var name = Guid.NewGuid().ToString("N") + ".jpg";
            var path = Path.Combine(directory, name);
            try
            {
                await image.WriteAsync(path, MagickFormat.Jpeg, ct);
            }
            catch
            {
                File.Delete(path);
                throw;
            }
            return UrlPrefix + name;
        }
        catch (MagickException)
        {
            throw new ImageUploadException("Bildet kunne ikke åpnes. Velg et annet JPG-, PNG- eller WebP-bilde.");
        }
        finally { processing.Release(); }
    }
}

public sealed class ImageUploadException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
