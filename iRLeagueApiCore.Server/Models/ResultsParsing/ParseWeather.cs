using System;

namespace iRLeagueApiCore.Server.Models.ResultsParsing
{
    public struct ParseWeather
    {
        public int type;
        public int temp_units;
        public int temp_value;
        public int rel_humidity;
        public int fog;
        public int wind_dir;
        public int wind_units;
        public int wind_value;
        public int skies;
        public int weather_var_initial;
        public int weather_var_ongoing;
        public int time_of_day;
        public DateTime simulated_start_utc_time;
        public long simulated_start_utc_offset;
    }
}
