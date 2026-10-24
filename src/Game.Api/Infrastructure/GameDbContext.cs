using Game.Api.Players;
using Game.Api.Seasons;
using Game.Api.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Infrastructure;

public class GameDbContext(DbContextOptions<GameDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Season>(season =>
        {
            season.HasKey(s => s.Id);
            season.Property(s => s.Id).HasConversion<SeasonIdConverter>();
            season.Property(s => s.Name).HasMaxLength(Season.MaxNameLength).IsRequired();
            season.HasIndex(s => new { s.StartsAt, s.EndsAt });
        });

        builder.Entity<Player>(player =>
        {
            player.HasKey(p => p.Id);
            player.Property(p => p.Id).HasConversion<PlayerIdConverter>();
            player.Property(p => p.SeasonId).HasConversion<SeasonIdConverter>();

            // A user can only join a given season once
            player.HasIndex(p => new { p.UserId, p.SeasonId }).IsUnique();

            player.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            player.HasOne<Season>().WithMany().HasForeignKey(p => p.SeasonId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}