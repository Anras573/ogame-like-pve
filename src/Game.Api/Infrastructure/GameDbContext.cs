using Game.Api.Planets;
using Game.Api.Players;
using Game.Api.Seasons;
using Game.Api.Users;
using Game.Core.Buildings;
using Game.Core.Research;
using Game.Core.Units;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Api.Infrastructure;

public class GameDbContext(DbContextOptions<GameDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Planet> Planets => Set<Planet>();

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

        builder.Entity<Planet>(planet =>
        {
            planet.HasKey(p => p.Id);
            planet.Property(p => p.Id).HasConversion<PlanetIdConverter>();
            planet.Property(p => p.PlayerId).HasConversion<PlayerIdConverter>();
            planet.Property(p => p.SeasonId).HasConversion<SeasonIdConverter>();
            planet.Property(p => p.Name).HasMaxLength(Planet.MaxNameLength).IsRequired();
            planet.Property(p => p.Version).IsRowVersion();

            planet.ComplexProperty(p => p.Resources);

            // Read-only views over the jsonb-backed fields below
            planet.Ignore(p => p.BuildingLevels);
            planet.Ignore(p => p.Units);
            planet.Ignore(p => p.ShipyardOrders);

            JsonProperty<Planet, Dictionary<BuildingType, int>>(planet, "_buildingLevels", "BuildingLevels");
            JsonProperty<Planet, Dictionary<UnitType, int>>(planet, "_units", "Units");
            JsonProperty<Planet, List<ShipyardOrder>>(planet, "_shipyardOrders", "ShipyardOrders");

            planet.OwnsOne(p => p.Construction, construction =>
            {
                construction.Property(c => c.Building).HasConversion<string>().HasMaxLength(50);

                // The worker looks up constructions that are due
                construction.HasIndex(c => c.CompletesAt);
            });

            // One planet per slot in a season's universe
            planet.HasIndex(p => new { p.SeasonId, p.Galaxy, p.SolarSystem, p.Position }).IsUnique();

            planet.HasOne<Player>().WithMany().HasForeignKey(p => p.PlayerId).OnDelete(DeleteBehavior.Cascade);
            planet.HasOne<Season>().WithMany().HasForeignKey(p => p.SeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Player>(player =>
        {
            player.HasKey(p => p.Id);
            player.Property(p => p.Id).HasConversion<PlayerIdConverter>();
            player.Property(p => p.SeasonId).HasConversion<SeasonIdConverter>();
            player.Property(p => p.Version).IsRowVersion();

            player.Ignore(p => p.ResearchLevels);
            JsonProperty<Player, Dictionary<ResearchType, int>>(player, "_researchLevels", "ResearchLevels");

            player.OwnsOne(p => p.CurrentResearch, research =>
            {
                research.Property(r => r.Research).HasConversion<string>().HasMaxLength(50);
                research.Property(r => r.PlanetId).HasConversion<PlanetIdConverter>();

                // The worker looks up research that is due
                research.HasIndex(r => r.CompletesAt);
            });

            // A user can only join a given season once
            player.HasIndex(p => new { p.UserId, p.SeasonId }).IsUnique();

            player.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            player.HasOne<Season>().WithMany().HasForeignKey(p => p.SeasonId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void JsonProperty<TEntity, TValue>(EntityTypeBuilder<TEntity> entity, string field, string column)
        where TEntity : class
        where TValue : new()
    {
        entity.Property<TValue>(field)
            .HasColumnName(column)
            .HasColumnType("jsonb")
            .HasConversion(new JsonColumnConverter<TValue>(), new JsonColumnComparer<TValue>());
    }
}