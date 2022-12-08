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
    [Collection("DbTestFixture")]
    public class ResultDbTestFixture : IClassFixture<DbTestFixture>
    {
        private DbTestFixture Fixture { get; }

        ILogger<ResultsController> MockLogger => new Mock<ILogger<ResultsController>>().Object;

        public ResultDbTestFixture(DbTestFixture fixture)
        {
            Fixture = fixture;
        }
    }
}
