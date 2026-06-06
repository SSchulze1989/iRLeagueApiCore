using System;
using System.Text.Json;

namespace iRLeagueDatabaseCore.Models;

public partial class DriverPointsEntryEntity
{
    public long LeagueId { get; set; }
    public long AccountEntryId { get; set; }
    public long AccountId { get; set; }
    public DateTime CreatedOn { get; set; }
    public long CreatedById { get; set; }
    public string Description { get; set; }
    public PenaltyValue Value { get; set; }
    public DateTime? ArchivedOn { get; set; }
    public string SourceRef { get; set; }
    public long? EntryKindId { get; set; }

    public virtual DriverPointsAccountEntity Account { get; set; }
    public virtual DriverPointsEntryKindEntity EntryKind { get; set; }
}

public class DriverPointsEntryEntityConfiguration : IEntityTypeConfiguration<DriverPointsEntryEntity>
{
    public void Configure(EntityTypeBuilder<DriverPointsEntryEntity> entity)
    {
        entity.ToTable("DriverPointsEntries");

        entity.HasKey(e => new { e.LeagueId, e.AccountEntryId });

        entity.HasAlternateKey(e => e.AccountEntryId);

        entity.Property(e => e.AccountEntryId)
            .ValueGeneratedOnAdd();

        entity.HasIndex(e => new { e.LeagueId, e.AccountId });

        entity.Property(e => e.Description).HasMaxLength(2048);
        entity.HasIndex(e => new { e.LeagueId, e.EntryKindId });

        entity.Property(e => e.Value)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                v => JsonSerializer.Deserialize<PenaltyValue>(v, default(JsonSerializerOptions)));

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Entries)
            .HasForeignKey(d => new { d.LeagueId, d.AccountId })
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(d => d.EntryKind)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.EntryKindId })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}