using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Events;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Events
{
    public class PutEventRequestValidator : AbstractValidator<PutEventRequest>
    {
        public PutEventRequestValidator(PutEventModelValidator eventValidator)
        {
            RuleFor(x => x.Event)
                .SetValidator(eventValidator);
        }
    }
}
