using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Results
{
    public class PutResultConfigRequestValidator : AbstractValidator<PutResultConfigRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PutResultConfigRequestValidator(LeagueDbContext dbContext, PutResultConfigModelValidator modelValidator)
        {
            this.dbContext = dbContext;
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
            RuleForEach(x => x.Model.Scorings)
                .MustAsync(ScoringIdValid);
        }

        private async Task<bool> ScoringIdValid(PutResultConfigRequest request, ScoringModel scoringModel, CancellationToken cancellationToken)
        {
            if (scoringModel.Id == 0)
            {
                return true;
            }
            var exists = await dbContext.Scorings
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.ScoringId == scoringModel.Id)
                .Select(x => new { x.ResultConfigId })
                .FirstOrDefaultAsync(cancellationToken);
            if (exists != null && exists.ResultConfigId == request.ResultConfigId)
            {
                return true;
            }
            return false;
        }
    }
}
