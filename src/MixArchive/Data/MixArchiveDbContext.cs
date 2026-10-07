using Microsoft.EntityFrameworkCore;
using MixArchive.Models;

namespace MixArchive.Data;

public class MixArchiveDbContext(DbContextOptions<MixArchiveDbContext> options) : DbContext(options)
{
    public DbSet<Mix> Mixes => Set<Mix>();

    public DbSet<MixFile> MixFiles => Set<MixFile>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<MixTag> MixTags => Set<MixTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
            throw new InvalidOperationException("ModelBuilder is null");

        modelBuilder.Entity<MixTag>().HasKey(mt => new { mt.MixId, mt.TagId });

        modelBuilder
            .Entity<MixTag>()
            .HasOne(mt => mt.Mix)
            .WithMany(m => m.MixTags)
            .HasForeignKey(mt => mt.MixId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<MixTag>()
            .HasOne(mt => mt.Tag)
            .WithMany(t => t.MixTags)
            .HasForeignKey(mt => mt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();

        modelBuilder
            .Entity<Mix>()
            .HasOne(m => m.File)
            .WithOne(f => f.Mix)
            .HasForeignKey<MixFile>(f => f.MixId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MixFile>().HasIndex(f => f.FileName).IsUnique();

        modelBuilder.Entity<MixFile>().HasIndex(f => f.FileHash);
    }
}
