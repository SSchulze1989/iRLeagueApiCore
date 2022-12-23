﻿using iRLeagueApiCore.Server.Handlers.Events;

namespace iRLeagueApiCore.Server.Validation.Events;

public class PutEventRequestValidator : AbstractValidator<PutEventRequest>
{
    public PutEventRequestValidator(PutEventModelValidator eventValidator)
    {
        RuleFor(x => x.Event)
            .SetValidator(eventValidator);
    }
}
