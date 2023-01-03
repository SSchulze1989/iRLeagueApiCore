namespace iRLeagueApiCore.Client.ResultsParsing;

public sealed class ParseCarClass
{
    public int car_class_id { get; set; }
    public ParseCarInClass[] cars_in_class { get; set; } = Array.Empty<ParseCarInClass>();
    public string? name { get; set; }
    public string? short_name { get; set; }
}
