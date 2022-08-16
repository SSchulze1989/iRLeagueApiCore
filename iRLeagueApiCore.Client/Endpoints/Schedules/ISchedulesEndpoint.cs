using iRLeagueApiCore.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Schedules
{
    public interface ISchedulesEndpoint : IPostGetAllEndpoint<ScheduleModel, PostScheduleModel>, IWithIdEndpoint<IScheduleByIdEndpoint>
    {
    }
}
