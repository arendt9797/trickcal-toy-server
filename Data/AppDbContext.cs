using Microsoft.EntityFrameworkCore;
using TrickcalServer.Models;

namespace TrickcalServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlayerAccount> Players => Set<PlayerAccount>();
    public DbSet<PlayerCharacter> PlayerCharacters => Set<PlayerCharacter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PlayerAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nickname).IsUnique();
            entity.Property(e => e.Nickname).HasMaxLength(20);
        });

        modelBuilder.Entity<PlayerCharacter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlayerId, e.CharacterId }).IsUnique();
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.Characters)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
