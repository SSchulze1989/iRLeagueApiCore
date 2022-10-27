using FluentValidation;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.EmailService;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Authentication
{
    public record PasswordResetRequest(PasswordResetModel Model) : IRequest;
    public class PasswordResetHandler : IRequestHandler<PasswordResetRequest, Unit>
    {
        private readonly ILogger<PasswordResetHandler> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEnumerable<IValidator<PasswordResetRequest>> validators;
        private readonly IEmailClient emailClient;

        public PasswordResetHandler(ILogger<PasswordResetHandler> logger, UserManager<ApplicationUser> userManager, 
            IEnumerable<IValidator<PasswordResetRequest>> validators, IEmailClient emailClient)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.validators = validators;
            this.emailClient = emailClient;
        }

        public async Task<Unit> Handle(PasswordResetRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var user = await userManager.FindByNameAsync(request.Model.UserName)
                ?? throw new ResourceNotFoundException();
            var token = await GetResetToken(user);
            await SendResetMailAsync(user, request.Model.Email, token);
            return Unit.Value;
        }

        private async Task<string> GetResetToken(ApplicationUser user)
        {
            return await userManager.GeneratePasswordResetTokenAsync(user);
        }

        private async Task SendResetMailAsync(ApplicationUser user, string email, string token)
        {
            string subject = $"Password Reset Token for your Account \"{user.UserName}\"";
            string body = GenerateMailBody(user, token);
            await emailClient.SendNoReplyMailAsync(email, subject, body);
        }

        private static string GenerateMailBody(ApplicationUser user, string token)
        {
            var resetUrl = GeneratePasswordResetUrl(user.Id, token);
            StringBuilder sb = new();
            sb.Append($@"
<p>Dear User,</p>
<p>For your account with the username ""{ user.UserName}"" a password reset was requested.
If you posted this request pleas use the following link to complete the process and set a new password:</p>
<a href=""{resetUrl}"">{resetUrl}</a>
<p>If you did not post this request you can ignore this mail.</p>
<p>Please do not reply to this mail. Messages send to the sender of this mail will not be processed</p>");
            return sb.ToString();
        }

        private static string GeneratePasswordResetUrl(string userId, string token)
        {
            return $"https://irleaguemanager.net/app/Auth/SetPassword/{userId}/{token}";
        }
    }
}
