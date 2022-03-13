﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    public interface IVersionModel
    {
        [DataMember]
        public DateTime? CreatedOn { get; set; }
        [DataMember]
        public DateTime? LastModifiedOn { get; set; }
        [DataMember]
        public string CreatedByUserId { get; set; }
        [DataMember]
        public string LastModifiedByUserId { get; set; }
        [DataMember]
        public string CreatedByUserName { get; set; }
        [DataMember]
        public string LastModifiedByUserName { get; set; }
    }
}
