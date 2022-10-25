﻿using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace iRLeagueApiCore.Services.EmailService
{
    public class ConfigEmailClientFactory : IEmailClientFactory
    {
        private readonly EmailClientConfiguration clientConfiguration;

        public ConfigEmailClientFactory(IConfiguration configuration)
        {
            clientConfiguration = ReadEmailConfig(configuration);
        }

        public IEmailClient CreateEmailClient()
        {
            // first detect if client config is default
            if (clientConfiguration.Equals(EmailClientConfiguration.Default))
            {
                // return mock client
                new IEmailClient()
            }
        }

        private static EmailClientConfiguration ReadEmailConfig(IConfiguration configuration)
        {
            var mailConfig = configuration.GetSection("Mail");
            if (mailConfig.Exists() == false)
            {
                return EmailClientConfiguration.Default;
            }

            string host = mailConfig["Host"] 
                ?? throw new InvalidOperationException("No value provided for \"Mail:Host\" in appsettings");
            if (int.TryParse(mailConfig["Port"], out int port) == false)
            {
                throw new InvalidOperationException("No value provided for \"Mail:Port\" in appsettings");
            }
            string? authUserName = mailConfig["UserName"];
            string? password = mailConfig["Password"];
            string sender = mailConfig["Sender"]
                ?? throw new InvalidOperationException("No value provided for \"Mail:Sender\" in appsettings");
            bool.TryParse(mailConfig["EnableSsl"], out bool enableSsl);

            NetworkCredential credentials;
            if (authUserName != null && password != null)
            {
                credentials = new NetworkCredential(authUserName, password);
            }
            else
            {
                credentials = new NetworkCredential();
            }

            return new EmailClientConfiguration(host, port, credentials, sender, enableSsl);
        }
    }
}
