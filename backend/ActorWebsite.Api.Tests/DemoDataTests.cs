using ActorWebsite.Api.Contracts;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using ActorWebsite.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ActorWebsite.Api.Tests;

public sealed class DemoDataTests
{
    [Fact]
    public async Task Seed_preserves_existing_content_and_edits_when_run_again()
    {
        await using var factory = new ApiFactory(development: true);
        using var client = await factory.ReadyClientAsync();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var original = new ActorEvent
        {
            Title = "Eksisterende arrangement",
            Description = "Behold dette innholdet.",
            Location = "Oslo",
            Address = "Oslo, Norge",
            StartsAt = DateTimeOffset.UtcNow.AddDays(90)
        };
        db.Events.Add(original);
        await db.SaveChangesAsync();

        Assert.Equal(0, await DemoDataCommand.RunAsync(factory.Services));
        var events = await db.Events.Where(item => item.Id != original.Id).ToListAsync();
        Assert.Equal(7, events.Count);
        Assert.Equal(5, events.Count(item => item.IsPublished && item.StartsAt > DateTimeOffset.UtcNow));
        Assert.Single(events, item => !item.IsPublished);
        Assert.Single(events, item => item.StartsAt < DateTimeOffset.UtcNow);
        var images = factory.Services.GetRequiredService<EventImageStore>();
        Assert.All(events, item =>
        {
            Assert.EndsWith("(demo)", item.Title);
            Assert.Empty(EventInput.From(item).Validate(images));
        });

        var bookings = await db.BookingRequests.ToListAsync();
        Assert.Equal(8, bookings.Count);
        Assert.Equal(3, bookings.Count(item => item.Status == "new"));
        Assert.Contains(bookings, item => item.EventType == "acting" && item.Status == "agreed");
        Assert.All(bookings, item =>
        {
            Assert.EndsWith("@example.test", item.Email);
            Assert.Matches("^[A-F0-9]{64}$", item.SubmissionHash);
        });
        events[0].Title = "Min endrede tittel";
        bookings[0].Status = "declined";
        await db.SaveChangesAsync();

        Assert.Equal(0, await DemoDataCommand.RunAsync(factory.Services));
        db.ChangeTracker.Clear();
        Assert.Equal(8, await db.Events.CountAsync());
        Assert.Equal(8, await db.BookingRequests.CountAsync());
        Assert.Equal("Eksisterende arrangement", (await db.Events.FindAsync(original.Id))!.Title);
        Assert.Equal("Min endrede tittel", (await db.Events.FindAsync(events[0].Id))!.Title);
        Assert.Equal("declined", (await db.BookingRequests.FindAsync(bookings[0].Id))!.Status);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Seed_refuses_to_write_outside_development()
    {
        await using var factory = new ApiFactory();
        using var client = await factory.ReadyClientAsync();
        Assert.Equal(1, await DemoDataCommand.RunAsync(factory.Services));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await db.Events.ToListAsync());
        Assert.Empty(await db.BookingRequests.ToListAsync());
    }
}
