namespace iRLeagueDatabaseCore.Models;

public partial class DriverPointsEntryKindEntity
{
    public long EntryKindId { get; set; }
    public long LeagueId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
    public virtual LeagueEntity League { get; set; }
}

public class DriverPointsEntryKindEntityConfiguration : IEntityTypeConfiguration<DriverPointsEntryKindEntity>
{
    public void Configure(EntityTypeBuilder<DriverPointsEntryKindEntity> entity)
    {
        entity.ToTable("DriverPointsEntryKinds");

        // Global key by EntryKindId so standard kinds can exist independent of league
        entity.HasKey(e => e.EntryKindId);

        entity.Property(e => e.EntryKindId)
            .ValueGeneratedOnAdd();

        entity.HasIndex(e => new { e.LeagueId, e.EntryType });

        entity.HasOne(d => d.League)
            .WithMany()
            .HasForeignKey(d => d.LeagueId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);

        entity.Property(e => e.Name).HasMaxLength(255).IsRequired(true);
        entity.Property(e => e.Description).HasMaxLength(2048);
    }
}