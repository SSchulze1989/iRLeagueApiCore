using FluentValidation;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Results
{
    public class PutResultConfigModelValidator : AbstractValidator<PutResultConfigModel>
    {
        public PutResultConfigModelValidator(PostResultConfigModelValidator includeValidator)
        {
            Include(includeValidator);
        }
    }
}
