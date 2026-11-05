using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Entities;
using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Infrastructure.Messaging;

namespace SmartPalm.AgronomyService.Infrastructure.Persistence;

public class AgronomyDbContext(DbContextOptions<AgronomyDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<TechnicalReport> TechnicalReports => Set<TechnicalReport>();
    public DbSet<UserProjection> Users => Set<UserProjection>();
    public DbSet<PlantationProjection> Plantations => Set<PlantationProjection>();
    public DbSet<SectorProjection> Sectors => Set<SectorProjection>();
    public DbSet<AgronomistAffiliationProjection> Affiliations => Set<AgronomistAffiliationProjection>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("agronomy");
        builder.Entity<Recommendation>(e =>
        {
            e.ToTable("recommendations"); e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Content).HasMaxLength(500).IsRequired(); e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20); e.HasIndex(x => new { x.SectorId, x.Status });
            e.HasIndex(x => new { x.AgronomistId, x.CreatedAtUtc }); e.HasIndex(x => x.ReportId);
            e.HasOne<TechnicalReport>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<TechnicalReport>(e =>
        {
            e.ToTable("technical_reports"); e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Title).HasMaxLength(200).IsRequired(); e.Property(x => x.Content).HasColumnType("text").IsRequired();
            e.Property(x => x.Findings).HasColumnType("text"); e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.SectorId, x.CreatedAtUtc }); e.HasIndex(x => x.InterventionId);
        });
        builder.Entity<UserProjection>(e => { e.ToTable("user_projections"); e.HasKey(x => x.UserId); e.Property(x => x.Role).HasMaxLength(40).IsRequired(); });
        builder.Entity<PlantationProjection>(e => { e.ToTable("plantation_projections"); e.HasKey(x => x.PlantationId); e.Property(x => x.Name).HasMaxLength(200).IsRequired(); });
        builder.Entity<SectorProjection>(e => { e.ToTable("sector_projections"); e.HasKey(x => x.SectorId); e.Property(x => x.Name).HasMaxLength(200).IsRequired(); e.HasIndex(x => x.PlantationId); });
        builder.Entity<AgronomistAffiliationProjection>(e => { e.ToTable("agronomist_affiliation_projections"); e.HasKey(x => x.AffiliationId); e.HasIndex(x => new { x.AgronomistId, x.PlantationId }).IsUnique(); });
        builder.Entity<InboxMessage>(e => { e.ToTable("inbox_messages"); e.HasKey(x => x.Id); e.Property(x => x.EventType).HasMaxLength(200).IsRequired(); });
        builder.Entity<OutboxMessage>(e => { e.ToTable("outbox_messages"); e.HasKey(x => x.Id); e.Property(x => x.EventType).HasMaxLength(200).IsRequired(); e.Property(x => x.Content).HasColumnType("jsonb").IsRequired(); e.Property(x => x.Error).HasMaxLength(2000); e.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc }); });
    }
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken token)
    {
        await using var transaction = await Database.BeginTransactionAsync(token);
        try { var result = await operation(token); await transaction.CommitAsync(token); return result; }
        catch { await transaction.RollbackAsync(token); throw; }
    }
}
