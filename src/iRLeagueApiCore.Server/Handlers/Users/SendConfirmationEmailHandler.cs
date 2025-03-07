﻿using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Services.EmailService;
using Microsoft.AspNetCore.Identity;

namespace iRLeagueApiCore.Server.Handlers.Users;

public record SendConfirmationEmailRequest(string Email, string LinkTemplate) : IRequest<Unit>;

public class SendConfirmationEmailHandler : UsersHandlerBase<SendConfirmationEmailHandler, SendConfirmationEmailRequest, Unit>
{
    private readonly IEmailClient emailClient;

    public SendConfirmationEmailHandler(ILogger<SendConfirmationEmailHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager,
        IEnumerable<IValidator<SendConfirmationEmailRequest>> validators, IEmailClient emailClient) : base(logger, userManager, validators)
    {
        this.emailClient = emailClient;
    }

    public override async Task<Unit> Handle(SendConfirmationEmailRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new ResourceNotFoundException();
        var token = await GetEmailConfirmationToken(user);
        await SendConfirmationMail(user, token, request.LinkTemplate);
        return Unit.Value;
    }

    private async Task SendConfirmationMail(ApplicationUser user, string token, string linkTemplate)
    {
        var subject = "Confirm your Emailaddress for iRLeagueManager.net";
        var body = GenerateMailBody(user, token, linkTemplate);
        if (user.Email is null)
        {
            throw new InvalidOperationException("Could not send confirmation email: User email was null");
        }
        await emailClient.SendNoReplyMailAsync(user.Email, subject, body);
    }
}
