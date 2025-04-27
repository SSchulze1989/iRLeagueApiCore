
namespace iRLeagueDatabaseCore.Models;
public partial class RosterEntity : Revision, IVersionEntity
{
    public long LeagueId { get; set; }
    public long RosterId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual LeagueEntity League { get; set; }
    public virtual ICollection<RosterEntryEntity> RosterEntries { get; set; }
}

public class RosterEntityConfiguration : IEntityTypeConfiguration<RosterEntity>
{
    public void Configure(EntityTypeBuilder<RosterEntity> entity)
    {
        entity.ToTable("Rosters");

        entity.HasKey(e => new { e.LeagueId, e.RosterId });
        
        entity.HasAlternateKey(e => e.RosterId);
        
        entity.Property(e => e.RosterId).ValueGeneratedOnAdd();

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.Description)
            .HasColumnType("text");

        entity.HasOne(d => d.League)
            .WithMany(p => p.Rosters)
            .HasForeignKey(d => d.LeagueId)
            .IsRequired(true);
    }
}
