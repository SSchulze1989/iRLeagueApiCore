using FluentAssertions;
using iRLeagueApiCore.TrackImport.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.TrackImport
{
    public class TrackImportServiceTests
    {
        [Fact]
        public void EncodePassword_ShouldEncodeTestHash_WhenUsingTestPassword()
        {
            string userName = "CLunky@iracing.Com";
            string password = "MyPassWord";
            string expected = "xGKecAR27ALXNuMLsGaG0v5Q9pSs2tZTZRKNgmHMg+Q=";

            var encoded = TrackImportService.EncodePassword(userName, password);

            encoded.Should().Be(expected);
        }
    }
}
