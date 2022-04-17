using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PostScoringRequestValidator : AbstractValidator<PostScoringRequest>
    {
        public PostScoringRequestValidator(PostScoringModelValidator modelValidator)
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("League id required");
            RuleFor(x => x.SeasonId)
                .NotEmpty()
                .WithMessage("Season id required");
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }
    }
}
