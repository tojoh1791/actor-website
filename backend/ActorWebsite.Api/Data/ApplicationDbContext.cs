using ActorWebsite.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ActorEvent> Events => Set<ActorEvent>();

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
    }
}
