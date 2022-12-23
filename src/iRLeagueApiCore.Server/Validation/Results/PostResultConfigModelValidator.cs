﻿using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Results;

public class PostResultConfigModelValidator : AbstractValidator<PostResultConfigModel>
{
    public PostResultConfigModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
    }
}
