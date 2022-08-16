﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Seasons
{
    public class PutSeasonModelValidator : AbstractValidator<PutSeasonModel>
    {
        public PutSeasonModelValidator()
        {
            RuleFor(x => x.SeasonName)
                .NotEmpty()
                .WithMessage("Season name required");
        }
    }
}
