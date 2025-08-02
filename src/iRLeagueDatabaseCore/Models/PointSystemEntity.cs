namespace iRLeagueDatabaseCore.Models;

public class PointSystemEntity : IVersionEntity
{
    public PointSystemEntity()
    {
        Scorings = new HashSet<ScoringEntity>();
        Events = new HashSet<EventEntity>();
        PointFilters = new HashSet<FilterOptionEntity>();
        ResultFilters = new HashSet<FilterOptionEntity>();
    }

    public long LeagueId { get; set; }
    public long PointSystemId { get; set; }
    public long ChampSeasonId { get; set; }
    public long? SourcePointSystemId { get; set; }

    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int ResultsPerTeam { get; set; }

    public virtual LeagueEntity League { get; set; }
    public virtual PointSystemEntity SourcePointSystem { get; set; }
    public virtual ChampSeasonEntity ChampSeason { get; set; }
    public virtual ICollection<ScoringEntity> Scorings { get; set; }
    public virtual IEnumerable<EventEntity> Events { get; set; }
    public virtual ICollection<FilterOptionEntity> PointFilters { get; set; }
    public virtual ICollection<FilterOptionEntity> ResultFilters { get; set; }

    #region version
    public DateTime? CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public int Version { get; set; }
    public string CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; }
    public string LastModifiedByUserId { get; set; }
    public string LastModifiedByUserName { get; set; }
    public bool IsArchived { get; set; }
    #endregion

}

public sealed class ResultConfigurationEntityConfiguration : IEntityTypeConfiguration<PointSystemEntity>
{
    public void Configure(EntityTypeBuilder<PointSystemEntity> entity)
    {
        entity.ToTable("ResultConfigurations");

        entity.HasKey(e => new { e.LeagueId, e.PointSystemId });

        entity.HasAlternateKey(e => e.PointSystemId);

        entity.Property(e => e.PointSystemId).HasColumnName("ResultConfigId");

        entity.Property(e => e.SourcePointSystemId).HasColumnName("SourceResultConfigId")
            .IsRequired(false);

        entity.Property(e => e.PointSystemId)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.CreatedOn).HasColumnType("datetime");

        entity.Property(e => e.LastModifiedOn).HasColumnType("datetime");

        entity.HasOne(d => d.League)
            .WithMany(p => p.ResultConfigs)
            .HasForeignKey(d => d.LeagueId);

        entity.HasOne(d => d.SourcePointSystem)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.SourcePointSystemId })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
