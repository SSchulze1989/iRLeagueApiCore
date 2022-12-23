using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Identity;

namespace iRLeagueApiCore.Server.Handlers.Users;

public record PutUserRequest(string UserId, PutUserModel Model) : IRequest<PrivateUserModel>;

public class PutUserHandler : UsersHandlerBase<PutUserHandler, PutUserRequest>, IRequestHandler<PutUserRequest, UserModel>
{
    public PutUserHandler(ILogger<PutUserHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager,
        IEnumerable<IValidator<PutUserRequest>> validators) : base(logger, userDbContext, userManager, validators)
    {
    }

    public async Task<UserModel> Handle(PutUserRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var user = await GetUserEntityAsync(request.UserId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        user = MapToUserEntity(request.Model, user);
        await userDbContext.SaveChangesAsync(cancellationToken);
        var getUser = MapToPrivateUserModel(user, new PrivateUserModel());
        return getUser;
    }

    protected ApplicationUser MapToUserEntity(PutUserModel model, ApplicationUser user)
    {
        user.FullName = GetUserFullName(model.Firstname, model.Lastname);
        user.Email = model.Email;
        return user;
    }
}
