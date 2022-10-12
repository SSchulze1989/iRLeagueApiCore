using FluentValidation;
using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Users
{
    public record SearchUsersByNameRequest(string[] SearchKeys) : IRequest<IEnumerable<UserModel>>;

    public class SearchUsersByNameHandler : UsersHandlerBase<SearchUsersByNameHandler, SearchUsersByNameRequest>, 
        IRequestHandler<SearchUsersByNameRequest, IEnumerable<UserModel>>
    {
        public SearchUsersByNameHandler(ILogger<SearchUsersByNameHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager, 
            IEnumerable<IValidator<SearchUsersByNameRequest>> validators) : base(logger, userDbContext, userManager, validators)
        {
        }

        public async Task<IEnumerable<UserModel>> Handle(SearchUsersByNameRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var users = await SearchUsers(request.SearchKeys);
            var getUsers = users.Select(x => MapToUserModel(x, new()));
            return getUsers;
        }

        private async Task<IEnumerable<ApplicationUser>> SearchUsers(params string[] searchKeys)
        {
            var regexString = string.Join('|', searchKeys.Select(x => x.ToLower()));
            return await userDbContext.Users
                .FromSqlInterpolated($"SELECT * FROM AspNetUsers WHERE LOWER(UserName) REGEXP {regexString} OR LOWER(FullName) REGEXP {regexString}")
                .ToListAsync();
        }
    }
}
