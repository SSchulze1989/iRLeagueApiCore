using System;

namespace iRLeagueApiCore.Server.Models.ResultsParsing
{
    public class ParseSessionResult
    {
        public int simsession_number { get; set; }
		public int simsession_type { get; set; }
		public string? simsession_type_name { get; set; }
        public int simsession_subtype { get; set; }
		public string? simsession_name { get; set; }
        public ParseSessionResultRow[] results { get; set; } = Array.Empty<ParseSessionResultRow>();
    }
}
