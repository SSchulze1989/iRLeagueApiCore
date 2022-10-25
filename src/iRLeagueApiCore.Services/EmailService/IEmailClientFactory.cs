using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.EmailService
{
    public interface IEmailClientFactory
    {
        public IEmailClient CreateEmailClient();
    }
}
