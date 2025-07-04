﻿namespace iRLeagueApiCore.Common.Models.Rosters;
public class PostRosterModel
{
    /// <summary>
    /// Name of the roster
    /// </summary>
    [DataMember]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// [Optional] Description of the roster
    /// </summary>
    [DataMember] 
    public string Description { get; set; } = string.Empty;
}
