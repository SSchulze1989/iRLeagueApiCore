using FluentValidation;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Admin
{
    public record PasswordResetRequest(PasswordResetModel Model) : IRequest;
    public class PasswordResetHandler : IRequestHandler<PasswordResetRequest, Unit>
    {
        private readonly ILogger<PasswordResetHandler> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEnumerable<IValidator<PasswordResetRequest>> validators;
        private readonly SmtpClient smtpClient;

        public PasswordResetHandler(ILogger<PasswordResetHandler> logger, UserManager<ApplicationUser> userManager, 
            IEnumerable<IValidator<PasswordResetRequest>> validators, SmtpClient smtpClient)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.validators = validators;
            this.smtpClient = smtpClient;
        }

        public async Task<Unit> Handle(PasswordResetRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var user = await userManager.FindByNameAsync(request.Model.UserName)
                ?? throw new ResourceNotFoundException();
            var token = await GetResetToken(user);
            SendResetMail(request.Model.Email, token);
            return Unit.Value;
        }

        private async Task<string> GetResetToken(ApplicationUser user)
        {
            return await userManager.GeneratePasswordResetTokenAsync(user);
        }

        private void SendResetMail(string email, string token)
        { 
            MailMessage mail = new();
            mail.From = new MailAddress("noreply@irleaguemanager.net");
            mail.To.Add(email);
            mail.Subject = "Password Reset";
            mail.Body = $"Reset Token: {token}";
            smtpClient.Send(mail);
        }
    }
}
