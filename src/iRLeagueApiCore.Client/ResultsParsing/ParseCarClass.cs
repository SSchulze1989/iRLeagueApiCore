namespace iRLeagueApiCore.Client.ResultsParsing
{
    public sealed class ParseCarClass
	{
        public int car_class_id { get; set; }
		public ParseCarInClass[] cars_in_class { get; set; } = Array.Empty<ParseCarInClass>();
		public int name { get; set; }
		public int short_name { get; set; }
    }
}
