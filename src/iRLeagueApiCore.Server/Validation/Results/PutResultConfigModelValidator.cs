using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Results;

public sealed class PutResultConfigModelValidator : AbstractValidator<PutPointSystemModel>
{
    public PutResultConfigModelValidator(PostResultConfigModelValidator includeValidator)
    {
        Include(includeValidator);
    }
}
