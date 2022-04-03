using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    public class LeagueControllerTests : IClassFixture<DbTestFixture>
    {
        DbTestFixture Fixture { get; }
        ITestOutputHelper Output { get; }
        ILogger<LeaguesController> MockLogger { get; }

        private const string testLeagueName = "TestLeague";
        private const string testFullName = "Full Name";
        private const long testLeagueId = 1;

        public LeagueControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            MockLogger = new Mock<ILogger<LeaguesController>>().Object;
        }

        private LeaguesController CreateController(IMediator mediator)
        {
            return AddContexts.AddAdminControllerContext(new LeaguesController(MockLogger, mediator));
        }

        private GetLeagueModel DefaultGetModel()
        {
            return new GetLeagueModel()
            {
                Id = testLeagueId,
                Name = testLeagueName,
                NameFull = testFullName,
            };
        }

        private PostLeagueModel DefaultPostModel()
        {
            return new PostLeagueModel()
            {
                Name = testLeagueName,
                NameFull = testFullName
            };
        }

        private PutLeagueModel DefaultPutModel()
        {
            return new PutLeagueModel()
            {
                NameFull = testFullName,
            };
        }

        [Fact]
        public async Task GetAll()
        {
            var expectedResult = new GetLeagueModel[] { DefaultGetModel() };
            var mediator = MockHelpers.TestMediator<GetLeaguesRequest, IEnumerable<GetLeagueModel>>(result: expectedResult);
            var controller = CreateController(mediator);
            var result = await controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetLeaguesRequest>(), default));
        }

        [Fact]
        public async Task Get()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<GetLeagueRequest, GetLeagueModel>(
                x => x.leagueId == testLeagueId, expectedResult);
            var controller = CreateController(mediator);
            var result = await controller.Get(testLeagueId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetLeagueRequest>(), default));
        }

        [Fact]
        public async Task Post()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PostLeagueRequest, GetLeagueModel>(
                x => x.Model.Name == testLeagueName, expectedResult);
            var controller = CreateController(mediator);
            var result = await controller.Post(DefaultPostModel());
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(expectedResult, createdResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PostLeagueRequest>(), default));
        }

        [Fact]
        public async Task Put()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PutLeagueRequest, GetLeagueModel>(
                x => x.LeagueId == testLeagueId, expectedResult);
            var controller = CreateController(mediator);
            var result = await controller.Put(testLeagueId, DefaultPutModel());
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PutLeagueRequest>(), default));
        }

        [Fact]
        public async Task Delete()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<DeleteLeagueRequest, Unit>(
                x => x.LeagueId == testLeagueId);
            var controller = CreateController(mediator);
            var result = await controller.Delete(testLeagueId);
            Assert.IsType<NoContentResult>(result);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<DeleteLeagueRequest>(), default));
        }
    }
}
