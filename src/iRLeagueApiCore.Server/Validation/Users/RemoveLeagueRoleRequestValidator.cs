using FluentValidation;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Users
{
    public class RemoveLeagueRoleRequestValidator : AbstractValidator<RemoveLeagueRoleRequest>
    {
        public RemoveLeagueRoleRequestValidator()
        {
            RuleFor(x => x.RoleName)
                .Must(LeagueRoles.IsValidRole)
                .WithMessage(request =>
                    $"Invalid value. Valid roles: [{string.Join(", ", LeagueRoles.RolesAvailable)}]");
        }
    }
}
