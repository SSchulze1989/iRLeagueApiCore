using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
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
    public class ScoringControllerTests
    {
        private readonly ILogger<ScoringsController> logger;

        const string testLeagueName = "TestLeague";
        const long testLeagueId = 1;
        const long testSeasonId = 1;
        const long testScoringId = 1;

        public ScoringControllerTests()
        {
            logger = Mock.Of<ILogger<ScoringsController>>();
        }

        private static GetScoringModel DefaultGetModel()
        {
            return new GetScoringModel()
            {
                Id = testScoringId,
                LeagueId = testLeagueId,
                SeasonId = testSeasonId,
                BasePoints = new double[0],
                BonusPoints = new string[0]
            };
        }

        private static PostScoringModel DefaultPostModel()
        {
            return new PostScoringModel();
        }

        private static PutScoringModel DefaultPutModel()
        {
            return new PutScoringModel();
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
        public async Task GetScoringValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<GetScoringRequest, GetScoringModel>(x =>
                x.LeagueId == testLeagueId && x.ScoringId == testScoringId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Get(testLeagueName, testLeagueId, testScoringId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetScoringRequest>(), default));
        }

        [Fact]
        public async Task GetScoringNotFound()
        {
            var mediator = MockHelpers.TestMediator<GetScoringRequest, GetScoringModel>(throws: NotFound());
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Get(testLeagueName, testLeagueId, testScoringId);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetScoringValidationFailed()
        {
            var mediator = MockHelpers.TestMediator<GetScoringRequest, GetScoringModel>(throws: ValidationFailed());
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Get(testLeagueName, testLeagueId, testScoringId);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostScoringValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PostScoringRequest, GetScoringModel>(x =>
                x.LeagueId == testLeagueId && x.SeasonId == testSeasonId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Post(testLeagueName, testLeagueId, testSeasonId, DefaultPostModel());
            var okResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PostScoringRequest>(), default));
        }

        [Fact]
        public async Task PostScoringNotFound()
        {
            var mediator = MockHelpers.TestMediator<PostScoringRequest, GetScoringModel>(throws: NotFound());
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Post(testLeagueName, testLeagueId, testSeasonId, DefaultPostModel());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostScoringValidationFailed()
        {
            var mediator = MockHelpers.TestMediator<PostScoringRequest, GetScoringModel>(throws: ValidationFailed());
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Post(testLeagueName, testLeagueId, testSeasonId, DefaultPostModel());
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PutScoringValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PutScoringRequest, GetScoringModel>(x =>
                x.LeagueId == testLeagueId && x.ScoringId == testScoringId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Put(testLeagueName, testLeagueId, testScoringId, DefaultPutModel());
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PutScoringRequest>(), default));
        }

        [Fact]
        public async Task PutScoringNotFound()
        {
            var mediator = MockHelpers.TestMediator<PutScoringRequest, GetScoringModel>(throws: NotFound());
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Put(testLeagueName, testLeagueId, testScoringId, DefaultPutModel());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutScoringValidationFailed()
        {
            var mediator = MockHelpers.TestMediator<PutScoringRequest, GetScoringModel>(throws: ValidationFailed());
            var controller = AddContexts.AddMemberControllerContext(new ScoringsController(logger, mediator));

            var result = await controller.Put(testLeagueName, testLeagueId, testScoringId, DefaultPutModel());
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
