using Microsoft.EntityFrameworkCore;
using MixArchive.Models;

namespace MixArchive.Data;

public class MixArchiveDbContext(DbContextOptions<MixArchiveDbContext> options) : DbContext(options)
{
    public DbSet<Mix> Mixes => Set<Mix>();

    public DbSet<MixFile> MixFiles => Set<MixFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
