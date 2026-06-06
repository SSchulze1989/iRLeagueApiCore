using System.Collections.Generic;

namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class CreateDriverPointsAccountModel
{
    public long MemberId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<long> SeasonIds { get; set; }
}