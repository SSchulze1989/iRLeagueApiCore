namespace iRLeagueDatabaseCore.Models;

public partial class DriverPointsAccountSeason
{
    public long LeagueId { get; set; }
    public long AccountId { get; set; }
    public long SeasonId { get; set; }

    public virtual DriverPointsAccountEntity Account { get; set; }
    public virtual SeasonEntity Season { get; set; }
}

public class DriverPointsAccountSeasonConfiguration : IEntityTypeConfiguration<DriverPointsAccountSeason>
{
    public void Configure(EntityTypeBuilder<DriverPointsAccountSeason> entity)
    {
        entity.ToTable("DriverPointsAccountSeasons");

        entity.HasKey(e => new { e.LeagueId, e.AccountId, e.SeasonId });

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Seasons)
            .HasForeignKey(d => new { d.LeagueId, d.AccountId })
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(d => d.Season)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.SeasonId })
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}