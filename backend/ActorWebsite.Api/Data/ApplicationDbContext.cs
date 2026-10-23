using ActorWebsite.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ActorEvent> Events => Set<ActorEvent>();
    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var entity = builder.Entity<ActorEvent>();
        entity.Property(e => e.Title).HasMaxLength(200);
        entity.Property(e => e.Description).HasMaxLength(10000);
        entity.Property(e => e.Location).HasMaxLength(200);
        entity.Property(e => e.Address).HasMaxLength(500);
        entity.Property(e => e.Organizer).HasMaxLength(200);
        entity.Property(e => e.ExternalUrl).HasMaxLength(2048);
        entity.Property(e => e.ImageUrl).HasMaxLength(2048);
        entity.HasIndex(e => new { e.IsPublished, e.StartsAt });
        var booking = builder.Entity<BookingRequest>();
        booking.Property(b => b.ContactName).HasMaxLength(100);
        booking.Property(b => b.Email).HasMaxLength(254);
        booking.Property(b => b.Phone).HasMaxLength(40);
        booking.Property(b => b.Organization).HasMaxLength(200);
        booking.Property(b => b.EventType).HasMaxLength(20);
        booking.Property(b => b.Location).HasMaxLength(300);
        booking.Property(b => b.Message).HasMaxLength(5000);
        booking.Property(b => b.Status).HasMaxLength(20);
        booking.Property(b => b.SubmissionHash).HasMaxLength(64);
        booking.HasIndex(b => new { b.Status, b.CreatedAt });
    }
}
