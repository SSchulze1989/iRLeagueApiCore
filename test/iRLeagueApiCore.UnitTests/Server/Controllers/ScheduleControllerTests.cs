using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.UnitTests.Server.Controllers;

[Collection("DbTestFixture")]
public class ScheduleDbTestFixture
{
    private readonly ILogger<SchedulesController> logger;

    const string testLeagueName = "TestLeague";
    const long testLeagueId = 1;
    const long testSeasonÍd = 1;
    const long testScheduleId = 1;
    const string testScheduleName = "Schedule 1";

    public ScheduleDbTestFixture()
    {
        logger = Mock.Of<ILogger<SchedulesController>>();
    }

    private static ScheduleModel DefaultGetModel()
    {
        return new ScheduleModel()
        {
            LeagueId = testLeagueId,
            ScheduleId = testScheduleId,
            Name = testScheduleName
        };
    }

    private static PostScheduleModel DefaultPostModel()
    {
        return new PostScheduleModel();
    }

    private static PutScheduleModel DefaultPutModel()
    {
        return new PutScheduleModel();
    }

    [Fact]
    public async Task GetSchedulesValid()
    {
        var expectedResult = new ScheduleModel[] { DefaultGetModel() };
        var mediator = MockHelpers.TestMediator<GetSchedulesRequest, IEnumerable<ScheduleModel>>(x =>
            x.LeagueId == testLeagueId, expectedResult);
        var controller = AddContexts.AddMemberControllerContext(new SchedulesController(logger, mediator));

        var result = await controller.GetAll(testLeagueName, testLeagueId);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResult, okResult.Value);
        var mediatorMock = Mock.Get(mediator);
        mediatorMock.Verify(x => x.Send(It.IsAny<GetSchedulesRequest>(), default));
    }

    [Fact]
    public async Task GetScheduleValid()
    {
        var expectedResult = DefaultGetModel();
        var mediator = MockHelpers.TestMediator<GetScheduleRequest, ScheduleModel>(x =>
            x.LeagueId == testLeagueId && x.ScheduleId == testScheduleId, expectedResult);
        var controller = AddContexts.AddMemberControllerContext(new SchedulesController(logger, mediator));

        var result = await controller.Get(testLeagueName, testLeagueId, testScheduleId);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResult, okResult.Value);
        var mediatorMock = Mock.Get(mediator);
        mediatorMock.Verify(x => x.Send(It.IsAny<GetScheduleRequest>(), default));
    }

    [Fact]
    public async Task PostScheduleValid()
    {
        var expectedResult = DefaultGetModel();
        var mediator = MockHelpers.TestMediator<PostScheduleRequest, ScheduleModel>(x =>
            x.LeagueId == testLeagueId, expectedResult);
        var controller = AddContexts.AddMemberControllerContext(new SchedulesController(logger, mediator));

        var result = await controller.Post(testLeagueName, testSeasonÍd, testLeagueId, DefaultPostModel());
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(expectedResult, createdResult.Value);
        var mediatorMock = Mock.Get(mediator);
        mediatorMock.Verify(x => x.Send(It.IsAny<PostScheduleRequest>(), default));
    }

    [Fact]
    public async Task PutScheduleValid()
    {
        var expectedResult = DefaultGetModel();
        var mediator = MockHelpers.TestMediator<PutScheduleRequest, ScheduleModel>(x =>
            x.LeagueId == testLeagueId && x.ScheduleId == testScheduleId, expectedResult);
        var controller = AddContexts.AddMemberControllerContext(new SchedulesController(logger, mediator));

        var result = await controller.Put(testLeagueName, testLeagueId, testScheduleId, DefaultPutModel());
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResult, okResult.Value);
        var mediatorMock = Mock.Get(mediator);
        mediatorMock.Verify(x => x.Send(It.IsAny<PutScheduleRequest>(), default));
    }

    [Fact]
    public async Task DeleteScheduleValid()
    {
        var mediator = MockHelpers.TestMediator<DeleteScheduleRequest, Unit>(x =>
            x.LeagueId == testLeagueId && x.ScheduleId == testScheduleId);
        var controller = AddContexts.AddMemberControllerContext(new SchedulesController(logger, mediator));

        var result = await controller.Delete(testLeagueName, testLeagueId, testScheduleId);
        Assert.IsType<NoContentResult>(result);
        var mediatorMock = Mock.Get(mediator);
        mediatorMock.Verify(x => x.Send(It.IsAny<DeleteScheduleRequest>(), default));
    }
}
