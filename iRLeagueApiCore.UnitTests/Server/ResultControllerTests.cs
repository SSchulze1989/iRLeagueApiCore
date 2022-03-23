﻿using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server
{
    public class ResultControllerTests : IClassFixture<DbTestFixture>
    {
        private DbTestFixture Fixture { get; }

        ILogger<ResultController> MockLogger => new Mock<ILogger<ResultController>>().Object;

        public ResultControllerTests(DbTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async void TestGetDefaultResult()
        {
            using(var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const long testResultId = 1;

                var controller = Fixture.AddControllerContext(new ResultController(MockLogger));
                var result = (await controller.Get(testLeagueName, testLeagueId, new long[] { testResultId },
                    dbContext)).Result;

                Assert.IsType<OkObjectResult>(result);
                var resultValue = (IEnumerable<GetResultModel>)((OkObjectResult)result).Value;
                var getResult = resultValue.First();

                Assert.Equal(testResultId, getResult.SessionId);
            }
        }
    }
}