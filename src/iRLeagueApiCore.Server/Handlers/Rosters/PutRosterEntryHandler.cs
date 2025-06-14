using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record PutRosterEntryRequest(long RosterId, long MemberId, RosterEntryModel Model) : IRequest<RosterEntryModel>;

public class PutRosterEntryHandler : RostersHandlerBase<PutRosterEntryHandler, PutRosterEntryRequest, RosterEntryModel>
{
    public PutRosterEntryHandler(ILogger<PutRosterEntryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutRosterEntryRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<RosterEntryModel> Handle(PutRosterEntryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);

        var roster = await GetRosterEntity(request.RosterId, cancellationToken)
            ?? throw new ResourceNotFoundException();

        var entry = roster.RosterEntries.FirstOrDefault(e => e.MemberId == request.MemberId);

        if (entry == null)
        {
            // Neuen Entry erstellen
            var member = await dbContext.LeagueMembers.FirstOrDefaultAsync(m => m.MemberId == request.MemberId, cancellationToken)
                ?? throw new ResourceNotFoundException();

            entry = new RosterEntryEntity
            {
                RosterId = roster.RosterId,
                Roster = roster,
                MemberId = request.MemberId,
                TeamId = request.Model.TeamId,
                Member = member
            };
            roster.RosterEntries.Add(entry);
        }
        else
        {
            // Vorhandenen Entry aktualisieren
            entry.TeamId = request.Model.TeamId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Optional: Mapping falls nötig
        var updatedEntry = new RosterEntryModel
        {
            MemberId = entry.MemberId,
            TeamId = entry.TeamId,
            Number = entry.Member.Number,
        };

        return updatedEntry;
    }
}
