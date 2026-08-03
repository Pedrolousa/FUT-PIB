using FutPib.Models;
using Microsoft.EntityFrameworkCore;

namespace FutPib.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<PlayerRating> Ratings => Set<PlayerRating>();
    public DbSet<WeeklySelection> WeeklySelections => Set<WeeklySelection>();
    public DbSet<TeamDraw> TeamDraws => Set<TeamDraw>();
    public DbSet<TeamDrawPlayer> TeamDrawPlayers => Set<TeamDrawPlayer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<PlayerRating>()
            .HasIndex(r => new { r.RaterUserId, r.RatedUserId })
            .IsUnique();

        modelBuilder.Entity<PlayerRating>()
            .HasOne(r => r.RaterUser)
            .WithMany()
            .HasForeignKey(r => r.RaterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PlayerRating>()
            .HasOne(r => r.RatedUser)
            .WithMany()
            .HasForeignKey(r => r.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WeeklySelection>()
            .HasIndex(s => new { s.WeekReference, s.UserId })
            .IsUnique();

        modelBuilder.Entity<TeamDrawPlayer>()
            .HasOne(x => x.TeamDraw)
            .WithMany(x => x.Players)
            .HasForeignKey(x => x.TeamDrawId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeamDrawPlayer>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
