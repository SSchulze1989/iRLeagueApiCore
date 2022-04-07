using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    [Collection("ControllerTests")]
    public class SessionControllerTests
    {
        ILogger<SessionsController> MockLogger { get; }

        public SessionControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            MockLogger = new Mock<ILogger<SessionsController>>().Object;
        }
    }
}
