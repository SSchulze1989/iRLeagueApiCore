using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace iRLeagueApiCore.Services.EmailService
{
    public class ConfigSmptClientFactory : ISmtpClientFactory
    {
        private readonly IConfiguration configuration;

        public ConfigSmptClientFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public SmtpClient GetSmtpClient()
        {
            var mailConfig = configuration.GetSection("Mail");
            var host = mailConfig["Host"];
            var port = mailConfig["Port"];
            var authUserName = mailConfig["UserName"];
            var password = mailConfig["Password"];

            SmtpClient client;
            if (port != null)
            {
                client = new(host, int.Parse(port));
            }
            else
            {
                client = new(host);
            }
            if (authUserName != null && password != null)
            {
                client.Credentials = new NetworkCredential(authUserName, password);
            }
            client.EnableSsl = true;

            return client;
        }
    }
}
