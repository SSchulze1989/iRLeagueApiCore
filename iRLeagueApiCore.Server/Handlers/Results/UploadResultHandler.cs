using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models.ResultsParsing;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public record UploadResultRequest(long leagueId, long EventId, Stream dataStream, IDictionary<int, int> Map) : IRequest<bool>;

    public class UploadResultHandler : HandlerBase<UploadResultHandler, UploadResultRequest>,
        IRequestHandler<UploadResultRequest, bool>
    {
        public UploadResultHandler(ILogger<UploadResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<UploadResultRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<bool> Handle(UploadResultRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            // decode file
            var data = await ParseDataStream(request.dataStream, cancellationToken);
            
            // add to database
            var @event = await GetEventEntityAsync(request.leagueId, request.EventId, cancellationToken) 
                ?? throw new ResourceNotFoundException();
            var result = await ReadResultsAsync(data, @event, request.Map, cancellationToken);
            @event.EventResult = result;
            await dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<EventEntity> GetEventEntityAsync(long leagueId, long eventId, CancellationToken cancellationToken)
        {
            // search for session first to check if result will be valid
            return await dbContext.Events
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Include(x => x.Sessions)
                .SingleOrDefaultAsync(cancellationToken);
        }

        private async Task<ParseSimSessionResult> ParseDataStream(Stream dataStream, CancellationToken cancellationToken)
        {
            return await JsonSerializer.DeserializeAsync<ParseSimSessionResult>(dataStream, cancellationToken: cancellationToken);
        }

        private async Task<EventResultEntity> ReadResultsAsync(ParseSimSessionResult data, EventEntity @event, IDictionary<int, int> map,
            CancellationToken cancellationToken)
        {
            // create entities
            var result = new EventResultEntity();
            var details = ReadDetails(data);
            IDictionary<int, SessionResultEntity> sessionResults = new Dictionary<int, SessionResultEntity>();
            foreach (var sessionResultData in data.session_results)
            {
                var sessionResult = await ReadSessionResultsAsync(data, sessionResultData, details, cancellationToken);
                sessionResults.Add(sessionResult);
            }
            var mappedSessionResults = MapToSubSessions(sessionResults, @event, map);
            foreach (var subResult in mappedSessionResults)
            {
                result.SessionResults.Add(subResult);
            }
            return result;
        }

        private async Task<MemberEntity> GetOrCreateMemberAsync(ParseSessionResultRow row, CancellationToken cancellationToken)
        {
            var member = await dbContext.Members
                .Where(x => x.IRacingId == row.cust_id.ToString())
                .SingleOrDefaultAsync();
            if (member == null)
            {
                var (firstname, Lastname) = GetFirstnameLastname(row.cust_id.ToString());
                member = new MemberEntity()
                {
                    Firstname = firstname,
                    Lastname = Lastname,
                    IRacingId = row.cust_id.ToString(),
                };
                dbContext.Members.Add(member);
            }
            return member;
        }

        private (string, string) GetFirstnameLastname(string name)
        {
            return name.Split(' ') switch { var a => (a[0], a[1]) };
        }

        private async Task<KeyValuePair<int, SessionResultEntity>> ReadSessionResultsAsync(ParseSimSessionResult sessionData, ParseSessionResult data, IRSimSessionDetailsEntity details,
            CancellationToken cancellationToken)
        {
            var sessionResult = new SessionResultEntity();
            sessionResult.IRSimSessionDetails = details;
            var laps = data.results.Max(x => x.laps_complete);
            var resultRows = new List<ResultRowEntity>();
            foreach (var row in data.results)
            {
                resultRows.Add(await ReadResultRowAsync(sessionData, row, laps, cancellationToken));
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
            details.EventAverageLap = data.event_average_lap;
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
            details.SimStartUtcOffset = data.weather.simulated_start_utc_offset;
            details.SimStartUtcTime = data.weather.simulated_start_utc_time;
            details.Skies = data.weather.skies;
            details.StartTime = data.start_time;
            details.TempUnits = data.weather.temp_units;
            details.TempValue = data.weather.temp_value;
            details.TimeOfDay = data.weather.time_of_day;
            details.TrackName = data.track.track_name;
            return details;
        }

        private async Task<ResultRowEntity> ReadResultRowAsync(ParseSimSessionResult sessionData, ParseSessionResultRow data, int laps,
            CancellationToken cancellationToken)
        {
            var row = new ResultRowEntity();

            row.AvgLapTime = data.average_lap;
            row.Car = "";
            row.CarClass = "";
            row.CarId = data.car_id;
            row.CarNumber = int.TryParse(data.livery.car_number, out int carNumber) ? carNumber : -1; // Todo: change to string!
            row.ClassId = data.car_class_id;
            row.ClubId = data.club_id;
            row.ClubName = data.club_name;
            row.CompletedLaps = data.laps_complete;
            row.CompletedPct = data.laps_complete / laps;
            row.ContactLaps = "";
            row.Division = data.division;
            row.FastestLapTime = data.best_lap_time;
            row.FastLapNr = data.best_lap_num;
            row.FinishPosition = data.position;
            row.Incidents = data.incidents;
            row.Interval = data.interval;
            row.IRacingId = data.cust_id.ToString();
            row.LeadLaps = data.laps_lead;
            row.License = sessionData.license_category;
            row.Member = await GetOrCreateMemberAsync(data, cancellationToken);
            row.NewCpi = data.new_cpi;
            row.NewIRating = data.newi_rating;
            row.NewLicenseLevel = data.new_license_level;
            row.NewSafetyRating = data.new_sub_level;
            row.NumContactLaps = -1;
            row.NumOfftrackLaps = -1;
            row.NumPitStops = -1;
            row.OfftrackLaps = "";
            row.OldCpi = data.old_cpi;
            row.OldIRating = data.oldi_rating;
            row.OldLicenseLevel = data.old_license_level;
            row.OldSafetyRating = data.old_sub_level;
            row.PittedLaps = "";
            row.PointsEligible = true;
            row.PositionChange = data.position - data.starting_position;
            row.QualifyingTime = data.qual_lap_time;
            row.QualifyingTimeAt = data.best_qual_lap_at;
            row.SimSessionType = -1;
            row.StartPosition = data.starting_position;
            row.Status = data.reason_out_id;
            
            return row;
        }

        private IEnumerable<SessionResultEntity> MapToSubSessions(IDictionary<int, SessionResultEntity> subResults, EventEntity @event, IDictionary<int, int> map)
        { 
            var mappedResults = new List<SessionResultEntity>();
            foreach(var pair in map)
            {
                var sessionResultNr = pair.Key;
                var sessionNr = pair.Value;
                if (subResults.ContainsKey(sessionResultNr) == false)
                {
                    throw new InvalidOperationException($"Error while trying to map subResult Nr.{sessionResultNr} to subSession Nr.{sessionNr}: no result with this subResultNr exists");
                }
                var sessionResult = subResults[sessionResultNr];
                var session = @event.Sessions
                    .SingleOrDefault(x => x.SessionNr == sessionNr)
                    ?? throw new InvalidOperationException($"Error while trying to map subResult Nr.{sessionResultNr} to subSession Nr.{sessionNr}: no subSession with this subSessionNr exists");
                sessionResult.Session = session;
                mappedResults.Add(sessionResult);
            }
            return mappedResults;
        }
    }
}
