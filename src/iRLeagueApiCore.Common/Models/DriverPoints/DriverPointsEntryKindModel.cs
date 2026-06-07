namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class DriverPointsEntryKindModel
{
    public long EntryKindId { get; set; }
    public long? LeagueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
}

public class CreateDriverPointsEntryKindModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
}

public class UpdateDriverPointsEntryKindModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public iRLeagueApiCore.Common.Enums.EntryType EntryType { get; set; }
}