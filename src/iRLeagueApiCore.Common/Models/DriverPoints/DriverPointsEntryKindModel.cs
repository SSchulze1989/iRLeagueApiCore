namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class DriverPointsEntryKindModel
{
    public long EntryKindId { get; set; }
    public long? LeagueId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
}

public class CreateDriverPointsEntryKindModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
}

public class UpdateDriverPointsEntryKindModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
}