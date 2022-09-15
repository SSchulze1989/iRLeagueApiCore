namespace iRLeagueApiCore.Server.Models.ResultsParsing
{
    public struct ParseCarClass
	{
        public int car_class_id { get; set; }
		public ParseCarInClass[] cars_in_class { get; set; }
		public int name { get; set; }
		public int short_name { get; set; }
    }
}
