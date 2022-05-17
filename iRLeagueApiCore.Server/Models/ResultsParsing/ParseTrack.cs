namespace iRLeagueApiCore.Server.Models.ResultsParsing
{
    public struct ParseTrack
    {
        public int track_id { get; set; }
        public string track_name { get; set; }
        public string config_name { get; set; }
        public int category_id { get; set; }
        public string category { get; set; }
    }
}
