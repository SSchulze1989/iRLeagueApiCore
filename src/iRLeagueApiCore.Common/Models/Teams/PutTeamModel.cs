﻿namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class PutTeamModel : PostTeamModel
{
    [DataMember]
    public bool IsArchived { get; set; }
}
