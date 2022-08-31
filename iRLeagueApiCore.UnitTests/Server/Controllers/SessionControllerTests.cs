using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    [Collection("ControllerTests")]
    public class SessionControllerTests
    {
        private readonly ILogger<EventsController> logger;

        const string testLeagueName = "TestLeague";
        const long testLeagueId = 1;
        const long testSessionId = 1;
        const string testSessionName = "Session 1";
        const long testScheduleId = 1;

        public SessionControllerTests()
        {
            logger = Mock.Of<ILogger<EventsController>>();
        }

        private static SessionModel DefaultGetModel()
        {
            return new SessionModel()
            {
                LeagueId = testLeagueId,
                SessionId = testSessionId,
                Name = testSessionName
            };
        }

        private static PostSessionModel DefaultPostModel()
        {
            return new PostSessionModel();
        }

        private static PutSessionModel DefaultPutModel()
        {
            return new PutSessionModel();
        }

        private static ResourceNotFoundException NotFound()
        {
            return new ResourceNotFoundException();
        }

        private static ValidationException ValidationFailed()
        {
            return new ValidationException("Test validation failed");
        }

        [Fact]
        public async Task GetSessionsValid()
        {
            var expectedResult = new SessionModel[] { DefaultGetModel() };
            var mediator = MockHelpers.TestMediator<GetSessionsRequest, IEnumerable<SessionModel>>(x =>
                x.LeagueId == testLeagueId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new EventsController(logger, mediator));

            var result = await controller.GetAll(testLeagueName, testLeagueId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetSessionsRequest>(), default));
        }

        [Fact]
        public async Task GetSessionValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<GetSessionRequest, SessionModel>(x =>
                x.LeagueId == testLeagueId && x.SessionId == testSessionId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new EventsController(logger, mediator));

            var result = await controller.Get(testLeagueName, testLeagueId, testSessionId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetSessionRequest>(), default));
        }

        [Fact]
        public async Task PostSessionToScheduleValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PostSessionToScheduleRequest, SessionModel>(x =>
                x.LeagueId == testLeagueId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new EventsController(logger, mediator));

            var result = await controller.PostToSchedule(testLeagueName, testLeagueId, testScheduleId, DefaultPostModel());
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(expectedResult, createdResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PostSessionToScheduleRequest>(), default));
        }

        [Fact]
        public async Task PutSessionValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PutSessionRequest, SessionModel>(x =>
                x.LeagueId == testLeagueId && x.SessionId == testSessionId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new EventsController(logger, mediator));

            var result = await controller.Put(testLeagueName, testLeagueId, testSessionId, DefaultPutModel());
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PutSessionRequest>(), default));
        }

        [Fact]
        public async Task DeleteSessionValid()
        {
            var mediator = MockHelpers.TestMediator<DeleteSessionRequest, Unit>(x =>
                x.LeagueId == testLeagueId && x.SessionId == testSessionId);
            var controller = AddContexts.AddMemberControllerContext(new EventsController(logger, mediator));

            var result = await controller.Delete(testLeagueName, testLeagueId, testSessionId);
            Assert.IsType<NoContentResult>(result);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<DeleteSessionRequest>(), default));
        }
    }
}
