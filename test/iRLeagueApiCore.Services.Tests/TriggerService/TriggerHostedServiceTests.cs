using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Services.TriggerService;
using iRLeagueApiCore.Services.TriggerService.Actions;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace iRLeagueApiCore.Services.Tests.TriggerService;
public sealed class TriggerHostedServiceTests : DataAccessTestsBase
{
    private readonly Mock<ILogger<TriggerHostedService>> loggerMock;
    private readonly IServiceCollection services;
    private readonly Mock<IBackgroundTaskQueue> taskQueueMock;

    public TriggerHostedServiceTests(ITestOutputHelper testOutputHelper)
    {
        var loggerMock = new Mock<ILogger<TriggerHostedService>>();
        loggerMock.Setup(x => x.BeginScope(It.IsAny<It.IsAnyType>()))
            .Returns(() => Mock.Of<IDisposable>());
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                var state = invocation.Arguments[2];
                var exception = (Exception?)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                var logMessage = (string?)invokeMethod?.Invoke(formatter, new[] { state, exception });

                testOutputHelper.WriteLine("{0}: {1}", logLevel, logMessage);
            }));
        this.loggerMock = loggerMock;
        taskQueueMock = new Mock<IBackgroundTaskQueue>();
        taskQueueMock.Setup(m => m.QueueBackgroundWorkItemAsync(It.IsAny<Func<CancellationToken, ValueTask>>()))
            .Returns<Func<CancellationToken, ValueTask>>(workItem => workItem(CancellationToken.None));
        fixture.Register(() => taskQueueMock.Object);
        var services = new ServiceCollection()
            .AddSingleton(LeagueProvider)
            .AddSingleton(dbContext)
            .AddSingleton<TriggerActionProvider>();
        this.services = services;
    }

    public override async Task InitializeAsync()
    {
        dbContext.Database.EnsureCreated();
        // only create league data for trigger tests
        var league = accessMockHelper.CreateLeague();
        dbContext.Leagues.Add(league);
        await dbContext.SaveChangesAsync();
    }

    private TriggerHostedService CreateSut(TriggerHostedServiceConfiguration? configuration = null)
    {
        configuration ??= new TriggerHostedServiceConfiguration
        {
            ScanTriggersInterval = TimeSpan.FromMilliseconds(50)
        };
        return new(loggerMock.Object, configuration, services.BuildServiceProvider(), taskQueueMock.Object);
    }

    [Fact]
    public void Should_CreateService_WithoutError()
    {
        var test = () => CreateSut();
        test.Should().NotThrow();
    }

    [Fact]
    public async Task Should_StartAndStopService_WithoutError()
    {
        var sut = CreateSut();
        var cts = new CancellationTokenSource();
        var test = async () =>
        {
            await sut.StartAsync(cts.Token);
            await Task.Delay(500);
            await sut.StopAsync(cts.Token);
            await Task.Delay(500);
        };
        await test.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    public async Task Should_ExecuteScantriggers_InSetInterval(int milliSeconds)
    {
        // arrange
        int executeCount = 0;
        int expectedCount = 10;
        taskQueueMock.Setup(m => m.QueueBackgroundWorkItemAsync(It.IsAny<Func<CancellationToken, ValueTask>>()))
            .Callback<Func<CancellationToken, ValueTask>>(async (workItem) =>
            {
                Interlocked.Increment(ref executeCount);
                await workItem(CancellationToken.None);
            })
            .Returns(ValueTask.CompletedTask);
        var trigger = new TriggerEntity()
        {
            TriggerType = TriggerType.Interval,
            Interval = TimeSpan.FromMilliseconds(50),
            TimeElapses = DateTimeOffset.Now.AddMilliseconds(50),
        };
        dbContext.Leagues.First().Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var configuration = new TriggerHostedServiceConfiguration
        {
            ScanTriggersInterval = TimeSpan.FromMilliseconds(milliSeconds)
        };
        var sut = CreateSut(configuration);
        var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);
        await Task.Delay(milliSeconds * expectedCount);
        await sut.StopAsync(cts.Token);

        // assert
        executeCount.Should().BeCloseTo(expectedCount, 1);
    }

    [Fact]
    public async Task Should_InvokeTimeTriggerAction_When_TriggerTimeElapses()
    {
        var expectedTime = DateTimeOffset.Now.AddMilliseconds(200);
        // arrange
        bool actionExecuted = false;
        taskQueueMock.Setup(m => m.QueueBackgroundWorkItemAsync(It.IsAny<Func<CancellationToken, ValueTask>>()))
            .Callback(() => actionExecuted = true)
            .Returns(ValueTask.CompletedTask);
        var trigger = new TriggerEntity()
        {
            TriggerType = TriggerType.Time,
            TimeElapses = expectedTime,
        };
        dbContext.Leagues.First().Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var sut = CreateSut();
        var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // assert
        await Task.Delay(100);
        actionExecuted.Should().BeFalse();
        await Task.Delay(200);
        actionExecuted.Should().BeTrue();

        // cleanup
        await sut.StopAsync(cts.Token);
    }

    [Fact]
    public async Task Should_InvokeIntervalTriggerAction_When_IntervalElapses()
    {
        TimeSpan interval = TimeSpan.FromMilliseconds(200);
        // arrange
        int executeCount = 0;
        taskQueueMock.Setup(m => m.QueueBackgroundWorkItemAsync(It.IsAny<Func<CancellationToken, ValueTask>>()))
            .Callback<Func<CancellationToken, ValueTask>>(async (workItem) =>
            {
                Interlocked.Increment(ref executeCount);
                await workItem(CancellationToken.None);
            })
            .Returns(ValueTask.CompletedTask);
        var trigger = new TriggerEntity()
        {
            TriggerType = TriggerType.Interval,
            Interval = interval,
            TimeElapses = DateTimeOffset.Now.Add(interval),
        };
        dbContext.Leagues.First().Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var sut = CreateSut();
        var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // assert
        await Task.Delay(50);
        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(interval);
            executeCount.Should().Be(i + 1);
        }

        // cleanup
        await sut.StopAsync(cts.Token);
    }

    [Fact]
    public async Task Should_InvokeEventTrigger_When_EventIsSend()
    {
        // arrange
        var executeCount = 0;
        taskQueueMock.Setup(m => m.QueueBackgroundWorkItemAsync(It.IsAny<Func<CancellationToken, ValueTask>>()))
           .Callback<Func<CancellationToken, ValueTask>>(async (workItem) =>
           {
               Interlocked.Increment(ref executeCount);
               await workItem(CancellationToken.None);
           })
           .Returns(ValueTask.CompletedTask);
        long resultRefId = fixture.Create<long>();
        var trigger1 = new TriggerEntity()
        {
            TriggerType = TriggerType.Event,
            EventType = TriggerEventType.ResultUploaded,
            RefId1 = null, // should be executed for any event
        };
        var trigger2 = new TriggerEntity()
        {
            TriggerType = TriggerType.Event,
            EventType = TriggerEventType.ResultUploaded,
            RefId1 = resultRefId, // should be executed only for event with id 5
        };
        var trigger3 = new TriggerEntity()
        {
            TriggerType = TriggerType.Event,
            EventType = TriggerEventType.ResultUploaded,
            RefId1 = fixture.Create<long>(), // should not be executed
        };
        var trigger4 = new TriggerEntity()
        {
            TriggerType = TriggerType.Event,
            EventType = TriggerEventType.ResultCalculated, // should not be executed
            RefId1 = null,
        };
        var league = dbContext.Leagues.First();
        league.Triggers.Add(trigger1);
        league.Triggers.Add(trigger2);
        league.Triggers.Add(trigger3);
        league.Triggers.Add(trigger4);
        await dbContext.SaveChangesAsync();
        accessMockHelper.SetCurrentLeague(league);

        var sut = CreateSut();

        // act
        await sut.ProcessEventTriggers(dbContext, TriggerEventType.ResultUploaded, new() { RefId1 = resultRefId }, CancellationToken.None);
        executeCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_InvokeManualTrigger()
    {
        // arrange
        bool actionExecuted = false;
        taskQueueMock.Setup(m => m.QueueBackgroundWorkItemAsync(It.IsAny<Func<CancellationToken, ValueTask>>()))
            .Callback(() => actionExecuted = true)
            .Returns(ValueTask.CompletedTask);
        var trigger = new TriggerEntity()
        {
            TriggerType = TriggerType.Manual,
        };
        dbContext.Leagues.First().Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();
        accessMockHelper.SetCurrentLeague(dbContext.Leagues.First());

        var sut = CreateSut();

        // act
        await sut.ProcessManualTrigger(dbContext, trigger.TriggerId, new(), CancellationToken.None);

        // assert
        actionExecuted.Should().BeTrue();
    }

    [Theory]
    [InlineData(TriggerAction.Webhook)]
    [InlineData(TriggerAction.Email)]
    [InlineData(TriggerAction.ApiCall)]
    public async Task Should_InvokeTriggerAction(TriggerAction action)
    {
        // arrange
        bool actionExecuted = false;
        var actionMock = new Mock<ITriggerAction>();
        actionMock.Setup(a => a.ExecuteAsync(It.IsAny<TriggerParameterModel>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .Callback(() => actionExecuted = true)
            .Returns(Task.CompletedTask);
        services.AddKeyedSingleton(action, actionMock.Object);
        var trigger = new TriggerEntity()
        {
            TriggerType = TriggerType.Manual,
            Action = action,
        };
        dbContext.Leagues.First().Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();
        accessMockHelper.SetCurrentLeague(dbContext.Leagues.First());

        var sut = CreateSut();

        // act
        await sut.ProcessManualTrigger(dbContext, trigger.TriggerId, new(), CancellationToken.None);
        await Task.Delay(100); // wait a bit for the action to be executed

        // assert
        actionExecuted.Should().BeTrue();
    }
}
