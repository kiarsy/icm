using ICMarkets.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ICMarkets.Infrastructure.Persistence;

public class IcMarketsDbContext(DbContextOptions<IcMarketsDbContext> options) : DbContext(options)
{
    public DbSet<BlockchainModel> BlockChainSnapshots => Set<BlockchainModel>();
    public DbSet<EventEnvelope> Events => Set<EventEnvelope>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<BlockchainModel>(entity =>
        {
            entity.ToTable("blockchain");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.BlockchainIdentifier).IsRequired();
            entity.Property(s => s.CreatedAt).HasConversion(utcConverter);
            entity.Property(s => s.UpdatedAt).HasConversion(utcConverter);
            entity.Property(s => s.Time).HasConversion(utcConverter);
            entity.Property(x => x.Revision).IsConcurrencyToken();
            entity.HasIndex(s => new { s.BlockchainIdentifier }).IsUnique();

        });

        modelBuilder.Entity<EventEnvelope>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.OccurredAt).HasConversion(utcConverter);
            entity.HasIndex(e => new { e.EventId, e.Version }).IsUnique();
        });
    }
}