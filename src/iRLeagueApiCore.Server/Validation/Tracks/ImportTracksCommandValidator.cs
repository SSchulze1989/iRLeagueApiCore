using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Tracks;

namespace iRLeagueApiCore.Server.Validation.Tracks
{
    public class ImportTracksCommandValidator : AbstractValidator<ImportTracksCommand>
    {
        public ImportTracksCommandValidator()
        {
            RuleFor(x => x.Model.UserName)
                .NotEmpty();
            RuleFor(x => x.Model.Password)
                .NotEmpty();
        }
    }
}
