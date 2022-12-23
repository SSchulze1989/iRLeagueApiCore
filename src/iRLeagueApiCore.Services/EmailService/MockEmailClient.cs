namespace iRLeagueApiCore.Services.EmailService;

public class MockEmailClient : IEmailClient
{
    public Task SendNoReplyMailAsync(string email, string subject, string body)
    {
        return Task.CompletedTask;
    }
}
