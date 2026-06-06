using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class DriverPointsAccountModel
{
    public long AccountId { get; set; }
    public long MemberId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(2048)]
    public string Description { get; set; }

    public DateTime CreatedOn { get; set; }
    public IEnumerable<DriverPointsEntryModel> ActiveEntries { get; set; }
    public double CurrentPoints { get; set; }
}
