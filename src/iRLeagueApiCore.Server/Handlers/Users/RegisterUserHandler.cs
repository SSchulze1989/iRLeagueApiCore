using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Services.EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Text;

namespace iRLeagueApiCore.Server.Handlers.Users;

public record RegisterUserRequest(RegisterModel Model, string LinkTemplate) : IRequest<(UserModel? user, IdentityResult result)>;

public enum UserRegistrationStatus
{
    Success,
    UserExists,
    CreateUserFailed,
}

public class RegisterUserHandler : UsersHandlerBase<RegisterUserHandler, RegisterUserRequest>, 
    IRequestHandler<RegisterUserRequest, (UserModel? user, IdentityResult result)>
{
    private readonly IEmailClient emailClient;

    public RegisterUserHandler(ILogger<RegisterUserHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager,
        IEnumerable<IValidator<RegisterUserRequest>> validators, IEmailClient emailClient) : base(logger, userDbContext, userManager, validators)
    {
        this.emailClient = emailClient;
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

        var emailConfirmationToken = await GetEmailConfirmationToken(user);
        await SendConfirmationMail(user, emailConfirmationToken, request.LinkTemplate);

        var userModel = MapToUserModel(user, new());
        return (userModel, IdentityResult.Success);
    }

    private async Task SendConfirmationMail(ApplicationUser user, string token, string linkTemplate)
    {
        var subject = "Confirm your Emailaddress for iRLeagueManager.net";
        var body = GenerateMailBody(user, token, linkTemplate);
        await emailClient.SendNoReplyMailAsync(user.Email, subject, body);
    }

    private async Task<string> GetEmailConfirmationToken(ApplicationUser user)
    {
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    private string GenerateMailBody(ApplicationUser user, string emailConfirmationToken, string linkTemplate)
    {
        var confirmUrl = GenerateEmailConfirmationLink(user.Id, emailConfirmationToken, linkTemplate);
        var body = $"""
            <p>Hello {user.UserName},</p>
            <p>Thank you for your registration with <a href="https://irleaguemanager.net">iRLeagueManager.net</a> to bring your league results hosting to the next level!</p>
            <p>
                To finish activation of your account we only need you to confirm your email adress by clicking the link below:<br/>
                <a href="{confirmUrl}">{confirmUrl}</a>
            </p>
            <p>After you finished the confirmation you can log into your account with your username and the password that you set when you registered on the webpage.</p>
            <small>
                In case you got this mail even if you did not register with yourself on iRLeagueManager.net or any connected service, please just ignore it.<br/>
                For further questions please contact <a href="mailto:simon@irleaguemanager.net">simon@irleaguemanager.net</a><br/>
                Please do not reply to this mail.
            </small>
            """;
        return body;
    }

    private string GenerateEmailConfirmationLink(string userId, string token, string template)
    {
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var url = template
            .Replace("{userId}", userId)
            .Replace("{token}", encodedToken);
        return url;
    }
}
