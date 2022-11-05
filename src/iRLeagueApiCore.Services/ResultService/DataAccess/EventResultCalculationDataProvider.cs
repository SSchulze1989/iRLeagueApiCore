using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal sealed class EventResultCalculationDataProvider : DatabaseAccessBase, IEventResultCalculationDataProvider
    {
        public EventResultCalculationDataProvider(ILeagueDbContext dbContext) : 
            base(dbContext)
        {
        }

        public async Task<EventResultCalculationData?> GetData(EventResultCalculationConfiguration config, CancellationToken cancellationToken)
        {
            return await dbContext.EventResults
                .Select(MapToEventResultCalculationDataExpression)
                .FirstOrDefaultAsync(x => x.EventId == config.EventId, cancellationToken);
        }

        private static Expression<Func<EventResultEntity, EventResultCalculationData>> MapToEventResultCalculationDataExpression => eventResult => new EventResultCalculationData()
        {
            LeagueId = eventResult.LeagueId,
            EventId = eventResult.EventId,
            SessionResults = eventResult.SessionResults.Select(sessionResult => new SessionResultCalculationData()
            {
                LeagueId = sessionResult.LeagueId,
                SessionId = sessionResult.SessionId,
                AcceptedReviewVotes = sessionResult.Session.IncidentReviews
                    .SelectMany(review => review.AcceptedReviewVotes)
                    .Select(vote => new AcceptedReviewVoteCalculationData()
                    {
                        DefaultPenalty = vote.VoteCategory == null ? 0 : vote.VoteCategory.DefaultPenalty,
                        MemberAtFaultId = vote.MemberAtFaultId,
                        ReviewId = vote.ReviewId,
                        VoteCategoryId = vote.VoteCategoryId,
                        ReviewVoteId = vote.ReviewVoteId,
                    }),
                ResultRows = sessionResult.ResultRows.Select(row => new ResultRowCalculationData()
                {
                    MemberId = row.MemberId,
                    Firstname = row.Member == null ? string.Empty : row.Member.Firstname,
                    Lastname = row.Member == null ? string.Empty : row.Member.Lastname,
                    TeamId = row.TeamId,
                    TeamName = row.Team == null ? string.Empty : row.Team.Name,
                    AddPenalty = null,
                    AvgLapTime = row.AvgLapTime,
                    Car = row.Car,
                    CarClass = row.CarClass,
                    CarId = row.CarId,
                    CarNumber = row.CarNumber,
                    ClassId = row.ClassId,
                    CompletedLaps = row.CompletedLaps,
                    CompletedPct = row.CompletedPct,
                    FastestLapTime = row.FastestLapTime,
                    FastLapNr = row.FastLapNr,
                    FinishPosition = row.FinishPosition,
                    Division = row.Division,
                    Incidents = row.Incidents,
                    Interval = row.Interval,
                    LeadLaps = row.LeadLaps,
                    License = row.License,
                    NewIrating = row.NewIRating,
                    NewLicenseLevel = row.NewLicenseLevel,
                    NewSafetyRating = row.NewSafetyRating,
                    OldIrating = row.OldIRating,
                    OldLicenseLevel = row.OldLicenseLevel,
                    OldSafetyRating = row.OldSafetyRating,
                    QualifyingTime = row.QualifyingTime,
                    PositionChange = row.PositionChange,
                    RacePoints = row.RacePoints,
                    SeasonStartIrating = row.SeasonStartIRating,
                    StartPosition = row.StartPosition,
                    Status = row.Status,
                    TotalPoints = row.RacePoints,
                })
            })
        };
    }
}
