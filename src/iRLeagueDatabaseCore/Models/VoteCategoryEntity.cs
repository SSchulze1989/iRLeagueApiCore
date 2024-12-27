#nullable disable

using System.Text.Json;

namespace iRLeagueDatabaseCore.Models;

public partial class VoteCategoryEntity
{
    public VoteCategoryEntity()
    {
        AcceptedReviewVotes = new HashSet<AcceptedReviewVoteEntity>();
        CommentReviewVotes = new HashSet<ReviewCommentVoteEntity>();
    }

    public long LeagueId { get; set; }
    public long CatId { get; set; }
    public string Text { get; set; }
    public int Index { get; set; }
    public PenaltyValue DefaultPenalty { get; set; }
    /// <summary>
    /// Imported Id from old database
    /// Will be deleted after imports have finished
    /// </summary>
    public long? ImportId { get; set; }

    public virtual LeagueEntity League { get; set; }
    public virtual ICollection<AcceptedReviewVoteEntity> AcceptedReviewVotes { get; set; }
    public virtual ICollection<ReviewCommentVoteEntity> CommentReviewVotes { get; set; }
}

public class VoteCategoryEntityConfiguration : IEntityTypeConfiguration<VoteCategoryEntity>
{
    public void Configure(EntityTypeBuilder<VoteCategoryEntity> entity)
    {
        entity.HasKey(e => new { e.LeagueId, e.CatId });

        entity.HasAlternateKey(e => e.CatId);

        entity.Property(e => e.CatId)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.DefaultPenalty)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                v => JsonSerializer.Deserialize<PenaltyValue>(v, default(JsonSerializerOptions)));

        entity.HasOne(e => e.League)
            .WithMany(p => p.VoteCategories)
            .HasForeignKey(e => e.LeagueId);
    }
}
