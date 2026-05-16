using Microsoft.EntityFrameworkCore;
using UrlShortener.App.Models.Entities;

namespace UrlShortener.App.Infrastructure.Persistence;

public sealed class UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.Property(x => x.OriginalUrl).IsRequired();
            entity.Property(x => x.ShortCode).IsRequired();
            entity.HasIndex(x => x.ShortCode).IsUnique();
            entity.Property(x => x.ShortUrlFull).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ClickCounter).IsRequired();

            entity.OwnsOne(x => x.Status, status =>
            {
                status.Property(s => s.Value)
                    .HasColumnName("Status")
                    .IsRequired();

                status.Property(s => s.Description)
                    .HasColumnName("StatusDescription");
            });
        });
    }
}
