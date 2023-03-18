using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Identity;

namespace iRLeagueApiCore.Server.Handlers.Users;

public record RegisterUserRequest(RegisterModel Model) : IRequest<(UserModel? user, IdentityResult result)>;

public enum UserRegistrationStatus
{
    Success,
    UserExists,
    CreateUserFailed,
}

public class RegisterUserHandler : UsersHandlerBase<RegisterUserHandler, RegisterUserRequest>, 
    IRequestHandler<RegisterUserRequest, (UserModel? user, IdentityResult result)>
{
    public RegisterUserHandler(ILogger<RegisterUserHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager, 
        IEnumerable<IValidator<RegisterUserRequest>> validators) : base(logger, userDbContext, userManager, validators)
    {
    }

    public async Task<(UserModel? user, IdentityResult result)> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var model = request.Model;
        _logger.LogInformation("Registering new user {UserName}", model.Username);
        var userExists = await userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            _logger.LogInformation("User {UserName} already exists", model.Username);
            return (null, IdentityResult.Failed(userManager.ErrorDescriber.DuplicateUserName(model.Username)));
        }

        ApplicationUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to add user {UserName} due to errors: {Errors}", model.Username, result.Errors
                .Select(x => $"{x.Code}: {x.Description}"));
            return (null, result);
        }

        _logger.LogInformation("User {UserName} created succesfully", model.Username);
        var userModel = MapToUserModel(user, new());
        return (userModel, IdentityResult.Success);
    }
}
