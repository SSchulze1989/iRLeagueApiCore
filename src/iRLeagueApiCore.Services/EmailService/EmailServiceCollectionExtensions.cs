﻿using iRLeagueApiCore.Services.EmailService;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Mail;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.TryAddScoped<IEmailClientFactory, ConfigEmailClientFactory>();
            services.TryAddTransient(sp => sp.GetRequiredService<IEmailClientFactory>().CreateEmailClient());

            return services;
        }
    }
}