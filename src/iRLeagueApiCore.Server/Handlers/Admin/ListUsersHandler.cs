using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Admin
{
    public record ListUsersRequest(string LeagueName) : IRequest<IEnumerable<GetAdminUserModel>>;

    public class ListUsersHandler : IRequestHandler<ListUsersRequest, IEnumerable<GetAdminUserModel>>
    {
        private readonly ILogger<ListUsersHandler> _logger;
        private readonly IEnumerable<IValidator<ListUsersRequest>> _validators;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListUsersHandler(ILogger<ListUsersHandler> logger, IEnumerable<IValidator<ListUsersRequest>> validators, 
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _validators = validators;
            _userManager = userManager;
        }

        public async Task<IEnumerable<GetAdminUserModel>> Handle(ListUsersRequest request, CancellationToken cancellationToken = default)
        {
            await _validators.ValidateAllAndThrowAsync(request, cancellationToken);
            // Get users that have a league role
            var users = await GetUsersWithLeagueRoleAsync(request.LeagueName);
            var getUsers = users.Select(x => MapToAdminUserModel(x.Key, x));
            return getUsers;
        }

        private async Task<IEnumerable<IGrouping<ApplicationUser, string>>> GetUsersWithLeagueRoleAsync(string leagueName)
        {
            var users = new List<(ApplicationUser user, string role)>();
            foreach (var role in LeagueRoles.RolesAvailable)
            {
                var leagueRoleName = LeagueRoles.GetLeagueRoleName(leagueName, role);
                var inRole = await _userManager.GetUsersInRoleAsync(leagueRoleName);
                if (inRole != null)
                {
                    users.AddRange(inRole.Select(user => (user, role)));
                }
            }
            return users.GroupBy(x => x.user, x => x.role);
        }

        private GetAdminUserModel MapToAdminUserModel(ApplicationUser user, IEnumerable<string> roles)
        {
            return new GetAdminUserModel()
            {
                UserName = user.UserName,
                Firsname = user.FullName?.Split(' ').First(),
                Lastname = user.FullName?.Split(' ').Last(),
                Email = user.Email,
                Roles = roles,
            };
        }
    }
}
