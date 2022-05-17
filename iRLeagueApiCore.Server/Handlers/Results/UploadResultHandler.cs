using FluentValidation;
using iRLeagueApiCore.Server.Models.ResultsParsing;
using iRLeagueDatabaseCore.Models;
using MediatR;
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
    public record UploadResultRequest(long leagueId, long ResultId, Stream dataStream, int sessionResultIndex) : IRequest<bool>;

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
            var result = ReadResults(data, request.sessionResultIndex);

            // Todo: add to database

            return true;
        }

        private async Task<ParseSimSessionResult> ParseDataStream(Stream dataStream, CancellationToken cancellationToken)
        {
            return await JsonSerializer.DeserializeAsync<ParseSimSessionResult>(dataStream, cancellationToken: cancellationToken);
        }

        private ResultEntity ReadResults(ParseSimSessionResult data, int sessionResultIndex)
        {
            // create entities
            var result = new ResultEntity();
            result.IRSimSessionDetails = ReadDetails(data);
            var sessionResultData = data.session_results.ElementAtOrDefault(sessionResultIndex);
            var laps = sessionResultData.results.Max(x => x.laps_complete);
            result.ResultRows = sessionResultData.results.Select(x => ReadResultRow(data, x, laps)).ToList();
            return result;
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

        private ResultRowEntity ReadResultRow(ParseSimSessionResult sessionData, ParseSessionResultRow data, int laps)
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
            row.IracingId = data.cust_id.ToString();
            row.LeadLaps = data.laps_lead;
            row.License = sessionData.license_category;
            row.NewCpi = data.new_cpi;
            row.NewIrating = data.newi_rating;
            row.NewLicenseLevel = data.new_license_level;
            row.NewSafetyRating = data.new_sub_level;
            row.NumContactLaps = -1;
            row.NumOfftrackLaps = -1;
            row.NumPitStops = -1;
            row.OfftrackLaps = "";
            row.OldCpi = data.old_cpi;
            row.OldIrating = data.oldi_rating;
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
    }
}
