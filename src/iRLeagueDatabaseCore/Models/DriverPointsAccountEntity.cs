namespace iRLeagueDatabaseCore.Models;

public partial class DriverPointsAccountEntity
{
    public DriverPointsAccountEntity()
    {
        Entries = new HashSet<DriverPointsEntryEntity>();
        Seasons = new HashSet<DriverPointsAccountSeason>();
    }

    public long LeagueId { get; set; }
    public long AccountId { get; set; }
    public long MemberId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedOn { get; set; }

    public virtual ICollection<DriverPointsEntryEntity> Entries { get; set; }
    public virtual ICollection<DriverPointsAccountSeason> Seasons { get; set; }
}

public class DriverPointsAccountEntityConfiguration : IEntityTypeConfiguration<DriverPointsAccountEntity>
{
    public void Configure(EntityTypeBuilder<DriverPointsAccountEntity> entity)
    {
        entity.ToTable("DriverPointsAccounts");

        entity.HasKey(e => new { e.LeagueId, e.AccountId });

        entity.HasAlternateKey(e => e.AccountId);

        entity.Property(e => e.AccountId)
            .ValueGeneratedOnAdd();

        entity.HasIndex(e => new { e.LeagueId, e.MemberId });

        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsRequired(true);

        entity.Property(e => e.Description)
            .HasMaxLength(2048);

        entity.Property(e => e.CreatedOn).HasColumnType("datetime");
    }
}