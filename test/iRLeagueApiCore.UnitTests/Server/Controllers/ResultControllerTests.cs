﻿using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.Extensions.Logging;

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
