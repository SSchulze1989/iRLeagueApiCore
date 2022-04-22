using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Seasons;
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
    public class SeasonControllerTests
    {
        private readonly ILogger<SeasonsController> logger;

        const string testLeagueName = "TestLeague";
        const long testLeagueId = 1;
        const long testSeasonId = 1;
        const string testSeasonName = "Season 1";

        public SeasonControllerTests()
        {
            logger = Mock.Of<ILogger<SeasonsController>>();
        }

        private static GetSeasonModel DefaultGetModel()
        {
            return new GetSeasonModel()
            {
                LeagueId = testLeagueId,
                SeasonId = testSeasonId,
                SeasonName = testSeasonName
            };
        }

        private static PostSeasonModel DefaultPostModel()
        {
            return new PostSeasonModel();
        }

        private static PutSeasonModel DefaultPutModel()
        {
            return new PutSeasonModel();
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
        public async Task GetSeasonsValid()
        {
            var expectedResult = new GetSeasonModel[] { DefaultGetModel() };
            var mediator = MockHelpers.TestMediator<GetSeasonsRequest, IEnumerable<GetSeasonModel>>(x =>
                x.LeagueId == testLeagueId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new SeasonsController(logger, mediator));

            var result = await controller.GetAll(testLeagueName, testLeagueId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetSeasonsRequest>(), default));
        }

        [Fact]
        public async Task GetSeasonValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<GetSeasonRequest, GetSeasonModel>(x =>
                x.LeagueId == testLeagueId && x.SeasonId == testSeasonId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new SeasonsController(logger, mediator));

            var result = await controller.Get(testLeagueName, testLeagueId, testSeasonId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GetSeasonRequest>(), default));
        }

        [Fact]
        public async Task PostSeasonValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PostSeasonRequest, GetSeasonModel>(x =>
                x.LeagueId == testLeagueId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new SeasonsController(logger, mediator));

            var result = await controller.Post(testLeagueName, testLeagueId, DefaultPostModel());
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(expectedResult, createdResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PostSeasonRequest>(), default));
        }

        [Fact]
        public async Task PutSeasonValid()
        {
            var expectedResult = DefaultGetModel();
            var mediator = MockHelpers.TestMediator<PutSeasonRequest, GetSeasonModel>(x =>
                x.LeagueId == testLeagueId && x.SeasonId == testSeasonId, expectedResult);
            var controller = AddContexts.AddMemberControllerContext(new SeasonsController(logger, mediator));

            var result = await controller.Put(testLeagueName, testLeagueId, testSeasonId, DefaultPutModel());
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResult, okResult.Value);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<PutSeasonRequest>(), default));
        }

        [Fact]
        public async Task DeleteSeasonValid()
        {
            var mediator = MockHelpers.TestMediator<DeleteSeasonRequest, Unit>(x =>
                x.LeagueId == testLeagueId && x.SeasonId == testSeasonId);
            var controller = AddContexts.AddMemberControllerContext(new SeasonsController(logger, mediator));

            var result = await controller.Delete(testLeagueName, testLeagueId, testSeasonId);
            Assert.IsType<NoContentResult>(result);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<DeleteSeasonRequest>(), default));
        }
    }
}
