using FluentValidation;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public record UploadResultRequest(long leagueId, long ResultId, string ResultJson) : IRequest<bool>;

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
            dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(request.ResultJson);
        }

        private async Task<ResultEntity> ParseResultsFromDynamicAsync(dynamic data)
        {
            // create entities
            var result = new ResultEntity();
        }

        private async Task<IRSimSessionDetailsEntity> ParseDetails(dynamic data)
        {
            var details = new IRSimSessionDetailsEntity();

            details.IRSessionId = Convert.ToInt64(data.session_id);
            details.IRSubsessionId = Convert.ToInt64(data.subsession_id);
            details.IRTrackId = Convert.ToInt32(data.track.track_id);
            details.ConfigName = data.track.config_name;
            details.Category = data.track.category;
            details.CornersPerLap = Convert.ToInt32(data.corners_per_lap);
            details.NumCautionLaps = Convert.ToInt32(data.num_caution_laps);
            details.NumCautions = Convert.ToInt32(data.num_cautions);
            details.NumLeadChanges = Convert.ToInt32(data.num_lead_changes);
            details.WarmupRubber = Convert.ToInt32(data.track_state.warmup_rubber);
            details.RaceRubber = Convert.ToInt32(data.track_stat.race_rubber);
            details.DamageModel = Convert.ToInt32(data.damage_model);
            details.EndTime = Convert.ToDateTime(data.weather.endTime);
            details.EventAverageLap = Convert.ToInt32(data.event_average_lap);
            details.EventLapsComplete = Convert.ToInt32(data.event_laps_complete);
            details.EventStrengthOfField = Convert.ToInt32(data.event_strength_of_field);
            details.Fog = Convert.ToInt32(data.weather.fog);
            details.IRRaceWeek = Convert.ToInt32(data.race_week);
            details.IRSeasonId = Convert.ToInt64(data.season_id);
            details.IRSeasonName = Convert.ToString(data.season_name);
            details.IRSeasonQuarter = Convert.ToInt32(data.season_quarter);
            details.IRSeasonYear = Convert.ToInt32(data.season_year);
            details.KmDistPerLap = 0;
            details.LeagueId = Convert.ToInt64(data.league_id);
            details.LeaveMarbles = Convert.ToInt32(data.track_state.leave_marbles);
            details.LicenseCategory = Convert.ToInt32(data.license_category);
            details.PracticeGripCompound = Convert.ToInt32(data.track_state.practice_grip_compound);
            details.PracticeRubber = Convert.ToInt32(data.track_state.practice_rubber);
            details.QualifyGripCompund = Convert.ToInt32(data.track_state.qualify_grip_compund);
            details.QualifyRubber = Convert.ToInt32(data.track_state.qualify_rubber);
            details.RaceGripCompound = Convert.ToInt32(data.track_state.race_grip_compound);
            details.RaceRubber = Convert.ToInt32(data.track_state.race_rubber);
            details.RelHumidity = Convert.ToInt32(data.track_state.rel_humidity);
            details.SessionName = Convert.ToString(data.session_name);
            details.SimStartUtcOffset = Convert.ToInt64(data.weather.simulated_start_utc_offset);
            details.SimStartUtcTime = Convert.ToDateTime(data.weather.simulated_start_utc_time);
            details.Skies = Convert.ToInt32(data.weather.skies);
            details.StartTime = Convert.ToDateTime(data.startTime);
            details.TempUnits = Convert.ToInt32(data.weather.temp_units);
            details.TempValue = Convert.ToInt32(data.weather.temp_value);
            details.TimeOfDay = Convert.ToInt32(data.timeOfDay);
            details.TrackName = Convert.ToString(data.track.track_name);
        }
    }
}
