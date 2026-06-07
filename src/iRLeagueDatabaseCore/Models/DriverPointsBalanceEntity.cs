namespace iRLeagueDatabaseCore.Models;

/// <summary>
/// Represents a balance (aggregate sum) of points for a specific entry kind.
/// Aggregates all PointEntries of the same kind for a driver's account.
/// </summary>
public partial class DriverPointsBalanceEntity
{
    public long LeagueId { get; set; }
    public long AccountId { get; set; }
    public long EntryKindId { get; set; }

    /// <summary>
    /// The aggregated balance value of all entries for this kind.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// The count of entries that make up this balance.
    /// </summary>
    public int EntryCount { get; set; }

    /// <summary>
    /// Last timestamp when this balance was updated.
    /// </summary>
    public DateTime LastUpdatedOn { get; set; }

    // Navigation properties
    public virtual DriverPointsAccountEntity Account { get; set; }
    public virtual DriverPointsEntryKindEntity EntryKind { get; set; }
}

public class DriverPointsBalanceEntityConfiguration : IEntityTypeConfiguration<DriverPointsBalanceEntity>
{
    public void Configure(EntityTypeBuilder<DriverPointsBalanceEntity> entity)
    {
        entity.ToTable("DriverPointsBalances");

        // Composite key: League + Account + EntryKind uniquely identifies a balance
        entity.HasKey(e => new { e.LeagueId, e.AccountId, e.EntryKindId });

        // Indexes for common queries
        entity.HasIndex(e => new { e.LeagueId, e.AccountId });
        entity.HasIndex(e => new { e.LeagueId, e.EntryKindId });

        entity.Property(e => e.Balance)
            .HasColumnType("decimal(18, 2)")
            .IsRequired(true);

        entity.Property(e => e.EntryCount)
            .IsRequired(true);

        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .IsRequired(true);

        // Foreign key to DriverPointsAccountEntity
        entity.HasOne(d => d.Account)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.AccountId })
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to DriverPointsEntryKindEntity
        entity.HasOne(d => d.EntryKind)
            .WithMany()
            .HasForeignKey(d => d.EntryKindId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Restrict);
    }
}