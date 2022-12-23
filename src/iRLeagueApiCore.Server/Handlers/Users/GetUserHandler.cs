using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Identity;

namespace iRLeagueApiCore.Server.Handlers.Users
{
    public record GetUserRequest(string UserId) : IRequest<UserModel>;

    public class GetUserHandler : UsersHandlerBase<GetUserHandler, GetUserRequest>,
        IRequestHandler<GetUserRequest, UserModel>
    {
        public GetUserHandler(ILogger<GetUserHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager,
            IEnumerable<IValidator<GetUserRequest>> validators) : base(logger, userDbContext, userManager, validators)
        {
        }

        public async Task<UserModel> Handle(GetUserRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var user = await userManager.FindByIdAsync(request.UserId)
                ?? throw new ResourceNotFoundException();
            var getUser = MapToUserModel(user, new());
            return getUser;
        }
    }
}
