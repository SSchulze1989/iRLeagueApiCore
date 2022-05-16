namespace iRLeagueApiCore.Server.Models.ResultsParsing
{
    public struct ParseSimSessionDetails
	{
        public long subsession_id;
        public long season_id;
        public string season_name;
        public string season_short_name;
        public int season_year;
        public int season_quarter;
        public int series_id;
        public string series_name;
        public string series_short_name;
        public int race_week_num;
        public long session_id;
        public int license_category_id;
        public string license_category;
        public long private_session_id;
        public long host_id;
        public string session_name;
        public long league_id;
        public string league_name;
        public long league_season_id;
        public string league_season_name;
        public bool restrict_results;
        public string start_time;
        public string end_time;
        public int num_laps_for_qual_average;
        public int num_laps_for_solo_average;
        public int corners_per_lap;
        public int caution_type;
        public int event_type;
        public string event_type_name;
        public bool driver_changes;
        public int min_team_drivers;
        public int max_team_drivers;
        public int driver_change_rule;
        public int driver_change_param1;
        public int driver_change_param2;
        public int max_weeks;
        public string points_type;
        public int event_strength_of_field;
        public int event_average_lap;
        public int event_laps_complete;
        public int num_cautions;
        public int num_caution_laps;
        public int num_lead_changes;
        public bool official_session;
        public int heat_info_id;
        public int special_event_type;
        public int damage_model;
        public bool can_protest;
        public int cooldown_minutes;
        public int limit_minutes;
        public ParseTrack track;
    }
}
