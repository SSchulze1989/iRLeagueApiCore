using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Client.ResultsParsing;
using iRLeagueApiCore.Services.ResultService.Excecution;
using System.Text.Json;
using System.Transactions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record UploadResultRequest(long leagueId, long EventId, ParseSimSessionResult ResultData) : IRequest<bool>;

public class UploadResultHandler : HandlerBase<UploadResultHandler, UploadResultRequest>,
    IRequestHandler<UploadResultRequest, bool>
{
    private readonly IResultCalculationQueue calculationQueue;
    private IDictionary<long, int> SeasonStartIratings;

    public UploadResultHandler(ILogger<UploadResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<UploadResultRequest>> validators,
        IResultCalculationQueue calculationQueue) :
        base(logger, dbContext, validators)
    {
        this.calculationQueue = calculationQueue;
        SeasonStartIratings = new Dictionary<long, int>();
    }

    public async Task<bool> Handle(UploadResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);

        // add to database
        var @event = await GetEventEntityAsync(request.leagueId, request.EventId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        SeasonStartIratings = await GetMemberSeasonStartIratingAsync(request.leagueId, @event.Schedule.SeasonId, cancellationToken);
        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            if (@event.EventResult is not null)
            {
                dbContext.Remove(@event.EventResult);
            }
            @event.SimSessionDetails.Clear();
            await dbContext.SaveChangesAsync(cancellationToken);
            var result = await ReadResultsAsync(request.ResultData, @event, cancellationToken);
            @event.EventResult = result;
            await dbContext.SaveChangesAsync(cancellationToken);
            tx.Complete();
        }
        // Queue result calculation for this event
        await calculationQueue.QueueEventResultAsync(@event.EventId);

        return true;
    }

    private async Task<EventEntity?> GetEventEntityAsync(long leagueId, long eventId, CancellationToken cancellationToken)
    {
        // search for session first to check if result will be valid
        return await dbContext.Events
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.EventId == eventId)
            .Include(x => x.Schedule)
            .Include(x => x.Sessions)
            .Include(x => x.EventResult)
                .ThenInclude(x => x.SessionResults)
                    .ThenInclude(x => x.IRSimSessionDetails)
            .Include(x => x.SimSessionDetails)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ParseSimSessionResult> ParseDataStream(Stream dataStream, CancellationToken cancellationToken)
    {
        return await JsonSerializer.DeserializeAsync<ParseSimSessionResult>(dataStream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to parse results from json");
    }

    private async Task<EventResultEntity> ReadResultsAsync(ParseSimSessionResult data, EventEntity @event,
        CancellationToken cancellationToken)
    {
        var map = CreateSessionMapping(data.session_results, @event);
        // create entities
        var result = @event.EventResult ?? new EventResultEntity();
        var details = ReadDetails(data);
        details.Event = @event;
        IDictionary<int, SessionResultEntity> sessionResults = new Dictionary<int, SessionResultEntity>();
        foreach (var sessionResultData in data.session_results)
        {
            var sessionResult = await ReadSessionResultsAsync(@event.LeagueId, data, sessionResultData, details, cancellationToken);
            sessionResults.Add(sessionResult);
        }
        var mappedSessionResults = MapToSubSessions(sessionResults, @event, map);
        foreach (var subResult in mappedSessionResults)
        {
            result.SessionResults.Add(subResult);
        }
        return result;
    }

    private static ICollection<(int resultNr, int sessionNr)> CreateSessionMapping(IEnumerable<ParseSessionResult> sessionResults, EventEntity @event)
    {
        var map = new List<(int resultNr, int sessionNr)>();

        var practiceSessionTypes = new[] { SimSessionType.OpenPractice }.Cast<int>();
        var practiceSession = @event.Sessions
            .FirstOrDefault(x => x.SessionType == SessionType.Practice);
        var practiceResult = sessionResults
            .FirstOrDefault(x => practiceSessionTypes.Contains(x.simsession_type) && x.simsession_name == "PRACTICE");
        if (practiceSession is not null && practiceResult is not null)
        {
            map.Add((practiceResult.simsession_number, practiceSession.SessionNr));
        }

        var qualySessionTypes = new[] { SimSessionType.LoneQualifying, SimSessionType.OpenQualifying }.Cast<int>();
        var qualySession = @event.Sessions
            .FirstOrDefault(x => x.SessionType == SessionType.Qualifying);
        var qualyResult = sessionResults
            .FirstOrDefault(x => qualySessionTypes.Contains(x.simsession_type));
        if (qualySession is not null && qualyResult is not null)
        {
            map.Add((qualyResult.simsession_number, qualySession.SessionNr));
        }

        var raceSessionTypes = new[] { SimSessionType.Race }.Cast<int>();
        var raceSessions = @event.Sessions
            .OrderBy(x => x.SessionNr)
            .Where(x => x.SessionType == SessionType.Race);
        var raceResults = sessionResults
            .Where(x => raceSessionTypes.Contains(x.simsession_type)).Reverse();
        foreach ((var session, var result) in raceSessions.Zip(raceResults))
        {
            if (session is not null && result is not null)
            {
                map.Add((result.simsession_number, session.SessionNr));
            }
        }

        return map;
    }

    private async Task<LeagueMemberEntity> GetOrCreateMemberAsync(long leagueId, ParseSessionResultRow row, CancellationToken cancellationToken)
    {
        var leagueMember = await dbContext.LeagueMembers
            .Include(x => x.Team)
            .Include(x => x.Member)
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.Member.IRacingId == row.cust_id.ToString())
            .SingleOrDefaultAsync(cancellationToken)
            ?? dbContext.LeagueMembers.Local
            .SingleOrDefault(x => x.Member.IRacingId == row.cust_id.ToString());
        if (leagueMember == null)
        {
            var league = await dbContext.Leagues
                .FirstAsync(x => x.Id == leagueId);
            var (firstname, lastname) = GetFirstnameLastname(row.display_name ?? string.Empty);
            var member = new MemberEntity()
            {
                Firstname = firstname,
                Lastname = lastname,
                IRacingId = row.cust_id.ToString(),
            };
            leagueMember = new LeagueMemberEntity()
            {
                Member = member,
                League = league,
            };
            dbContext.LeagueMembers.Add(leagueMember);
        }
        return leagueMember;
    }

    private static (string, string) GetFirstnameLastname(string name)
    {
        var parts = name.Split(' ', 2);
        var fullName = (parts[0], parts.ElementAt(1) ?? string.Empty);
        return fullName;
    }

    private async Task<KeyValuePair<int, SessionResultEntity>> ReadSessionResultsAsync(long leagueId, ParseSimSessionResult sessionData, ParseSessionResult data,
        IRSimSessionDetailsEntity details, CancellationToken cancellationToken)
    {
        var sessionResult = new SessionResultEntity();
        sessionResult.LeagueId = leagueId;
        sessionResult.IRSimSessionDetails = details;
        sessionResult.SimSessionType = (SimSessionType)data.simsession_type;
        var laps = data.results.Max(x => x.laps_complete);
        var resultRows = new List<ResultRowEntity>();
        foreach (var row in data.results)
        {
            resultRows.Add(await ReadResultRowAsync(leagueId, sessionData, row, laps, cancellationToken));
        }
        sessionResult.ResultRows = resultRows;
        var sessionResultNr = data.simsession_number;
        return new KeyValuePair<int, SessionResultEntity>(sessionResultNr, sessionResult);
    }

    private IRSimSessionDetailsEntity ReadDetails(ParseSimSessionResult data)
    {
        var details = new IRSimSessionDetailsEntity();

        details.IRSessionId = data.session_id;
        details.IRSubsessionId = data.subsession_id;
        details.IRTrackId = data.track.track_id;
        details.ConfigName = data.track.config_name;
        details.Category = data.track.category;
        details.CornersPerLap = data.corners_per_lap;
        details.NumCautionLaps = data.num_caution_laps;
        details.NumCautions = data.num_cautions;
        details.NumLeadChanges = data.num_lead_changes;
        details.WarmupRubber = data.track_state.warmup_rubber;
        details.RaceRubber = data.track_state.race_rubber;
        details.DamageModel = data.damage_model;
        details.EndTime = data.end_time;
        details.EventAverageLap = ParseTime(data.event_average_lap);
        details.EventLapsComplete = data.event_laps_complete;
        details.EventStrengthOfField = data.event_strength_of_field;
        details.Fog = data.weather.fog;
        details.IRRaceWeek = data.race_week_num;
        details.IRSeasonId = data.season_id;
        details.IRSeasonName = data.season_name;
        details.IRSeasonQuarter = data.season_quarter;
        details.IRSeasonYear = data.season_year;
        details.KmDistPerLap = 0;
        details.LeagueId = data.league_id;
        details.LeaveMarbles = data.track_state.leave_marbles;
        details.LicenseCategory = data.license_category_id;
        details.PracticeGripCompound = data.track_state.practice_grip_compound;
        details.PracticeRubber = data.track_state.practice_rubber;
        details.QualifyGripCompund = data.track_state.qualify_grip_compound;
        details.QualifyRubber = data.track_state.qualify_rubber;
        details.RaceGripCompound = data.track_state.race_grip_compound;
        details.RaceRubber = data.track_state.race_rubber;
        details.RelHumidity = data.weather.rel_humidity;
        details.SessionName = data.session_name;
        details.SimStartUtcOffset = ParseTime(data.weather.simulated_start_utc_offset);
        details.SimStartUtcTime = data.weather.simulated_start_utc_time;
        details.Skies = data.weather.skies;
        details.StartTime = data.start_time;
        details.TempUnits = data.weather.temp_units;
        details.TempValue = data.weather.temp_value;
        details.TimeOfDay = data.weather.time_of_day;
        details.TrackName = data.track.track_name;
        return details;
    }

    private async Task<ResultRowEntity> ReadResultRowAsync(long leagueId, ParseSimSessionResult sessionData, ParseSessionResultRow data,
        int laps, CancellationToken cancellationToken)
    {
        var row = new ResultRowEntity();
        var leagueMember = await GetOrCreateMemberAsync(leagueId, data, cancellationToken);
        row.LeagueId = leagueId;
        row.AvgLapTime = ParseTime(data.average_lap);
        row.Car = "";
        row.CarClass = "";
        row.CarId = data.car_id;
        row.CarNumber = int.TryParse(data.livery.car_number, out int carNumber) ? carNumber : -1; // Todo: change to string!
        row.ClassId = data.car_class_id;
        row.ClubId = data.club_id;
        row.ClubName = data.club_name;
        row.CompletedLaps = data.laps_complete;
        row.CompletedPct = laps != 0 ? data.laps_complete / (double)laps : 0;
        row.ContactLaps = "";
        row.Division = data.division;
        row.FastestLapTime = ParseTime(data.best_lap_time);
        row.FastLapNr = data.best_lap_num;
        row.FinishPosition = data.position;
        row.Incidents = data.incidents;
        row.Interval = ParseInterval(data.interval, data.laps_complete, laps);
        row.IRacingId = data.cust_id.ToString();
        row.LeadLaps = data.laps_lead;
        row.License = sessionData.license_category;
        row.Member = leagueMember.Member;
        row.NewCpi = (int)data.new_cpi;
        row.NewIRating = data.newi_rating;
        row.NewLicenseLevel = data.new_license_level;
        row.NewSafetyRating = data.new_sub_level;
        row.NumContactLaps = -1;
        row.NumOfftrackLaps = -1;
        row.NumPitStops = -1;
        row.OfftrackLaps = "";
        row.OldCpi = (int)data.old_cpi;
        row.OldIRating = data.oldi_rating;
        row.OldLicenseLevel = data.old_license_level;
        row.OldSafetyRating = data.old_sub_level;
        row.PittedLaps = "";
        row.PointsEligible = true;
        row.PositionChange = data.position - data.starting_position;
        row.QualifyingTime = ParseTime(data.qual_lap_time);
        row.QualifyingTimeAt = data.best_qual_lap_at;
        row.SimSessionType = -1;
        row.StartPosition = data.starting_position + 1;
        row.Status = data.reason_out_id;
        row.Team = leagueMember.Team;
        row.RacePoints = data.champ_points;
        row.SeasonStartIRating = SeasonStartIratings.TryGetValue(row.Member.Id, out int irating) ? irating : row.OldIRating;

        return row;
    }

    private async Task<IDictionary<long, int>> GetMemberSeasonStartIratingAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
    {
        return (await dbContext.ResultRows
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SubResult.Result.Event.Schedule.SeasonId == seasonId)
            .OrderBy(x => x.SubResult.Result.Event.Date)
            .Select(x => new { x.MemberId, x.OldIRating })
            .ToListAsync(cancellationToken))
            .DistinctBy(x => x.MemberId)
            .ToDictionary(k => k.MemberId, k => k.OldIRating);
    }

    private IEnumerable<SessionResultEntity> MapToSubSessions(IDictionary<int, SessionResultEntity> subResults, EventEntity @event,
        ICollection<(int resultNr, int sessionNr)> map)
    {
        var mappedResults = new List<SessionResultEntity>();
        foreach ((var resultNr, var sessionNr) in map)
        {
            if (subResults.ContainsKey(resultNr) == false)
            {
                throw new InvalidOperationException($"Error while trying to map subResult Nr.{resultNr} to subSession Nr.{sessionNr}: no result with this subResultNr exists");
            }
            var sessionResult = subResults[resultNr];
            var session = @event.Sessions
                .SingleOrDefault(x => x.SessionNr == sessionNr)
                ?? throw new InvalidOperationException($"Error while trying to map subResult Nr.{resultNr} to subSession Nr.{sessionNr}: no subSession with this subSessionNr exists");
            sessionResult.Session = session;
            session.SessionResult = sessionResult;
            mappedResults.Add(sessionResult);
        }
        return mappedResults;
    }

    private static TimeSpan ParseTime(int value) => TimeSpan.FromSeconds(value / 10000D);

    private static TimeSpan ParseTime(long value) => TimeSpan.FromSeconds(value / 10000D);

    private static TimeSpan ParseInterval(int value, int completedLaps, int sessionLaps)
    {
        if (value >= 0)
        {
            return TimeSpan.FromSeconds(value / 10000D);
        }

        return TimeSpan.FromDays(sessionLaps - completedLaps);
    }
}
