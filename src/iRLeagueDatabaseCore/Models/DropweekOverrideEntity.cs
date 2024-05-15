namespace iRLeagueDatabaseCore.Models;

public partial class DropweekOverrideEntity
{
    public long LeagueId { get; set; }
    public long StandingConfigId { get; set; }
    public long ScoredResultRowId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool ShouldDrop { get; set; }

    public virtual StandingConfigurationEntity StandingConfig { get; set; }
    public virtual ScoredResultRowEntity ScoredResultRow { get; set; }
}

public sealed class DropweekOverrideEntityConfiguration : IEntityTypeConfiguration<DropweekOverrideEntity>
{
    public void Configure(EntityTypeBuilder<DropweekOverrideEntity> entity)
    {
        entity.ToTable("DropweekOverrides");

        entity.HasKey(e => new { e.LeagueId, e.StandingConfigId, e.ScoredResultRowId });

        entity.HasOne(d => d.StandingConfig)
            .WithMany(p => p.DropweekOverrides)
            .HasForeignKey(d => new { d.LeagueId, d.StandingConfigId });

        entity.HasOne(d => d.ScoredResultRow)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.ScoredResultRowId });
    }
}