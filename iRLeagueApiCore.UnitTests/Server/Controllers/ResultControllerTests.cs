using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    [Collection("ControllerTests")]
    public class ResultControllerTests : IClassFixture<DbTestFixture>
    {
        private DbTestFixture Fixture { get; }

        ILogger<ResultsController> MockLogger => new Mock<ILogger<ResultsController>>().Object;

        public ResultControllerTests(DbTestFixture fixture)
        {
            Fixture = fixture;
        }
    }
}
