namespace iRLeagueDatabaseCore.Models;

public partial class DriverPointsExpiryRuleEntity
{
    public long LeagueId { get; set; }
    public long ExpiryRuleId { get; set; }
    public PenaltyType? PenaltyType { get; set; }
    // New: EntryKindId for configurable entry kinds (FK to DriverPointsEntryKind)
    public long? EntryKindId { get; set; }
    public int? IntervalDays { get; set; }
    public int? EventsDriven { get; set; }
    public int? EventsMissed { get; set; }
    public bool ResetOnNewSeason { get; set; }
    public string Description { get; set; }
}

public class DriverPointsExpiryRuleEntityConfiguration : IEntityTypeConfiguration<DriverPointsExpiryRuleEntity>
{
    public void Configure(EntityTypeBuilder<DriverPointsExpiryRuleEntity> entity)
    {
        entity.ToTable("DriverPointsExpiryRules");

        entity.HasKey(e => new { e.LeagueId, e.ExpiryRuleId });

        entity.HasAlternateKey(e => e.ExpiryRuleId);

        entity.Property(e => e.ExpiryRuleId)
            .ValueGeneratedOnAdd();

        entity.HasIndex(e => new { e.LeagueId, e.PenaltyType });

        entity.Property(e => e.Description).HasMaxLength(2048);
    }
}