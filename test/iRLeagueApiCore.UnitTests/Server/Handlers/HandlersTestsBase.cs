using FluentIdentityBuilder;
using FluentValidation;
using FluentValidation.Results;
using iRLeagueApiCore.Common;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace iRLeagueApiCore.UnitTests.Server.Handlers;

public abstract class HandlersTestsBase<THandler, TRequest, TResult> : DataAccessTestsBase
    where THandler : IRequestHandler<TRequest, TResult>
    where TRequest : class, IRequest<TResult>
{
    protected readonly ILogger<THandler> logger;

    protected const object? defaultId = null;

    protected long TestLeagueId => dbContext.Leagues.First().Id;
    protected const string testLeagueName = "TestLeague";
    protected long TestSeasonId => dbContext.Seasons.First().SeasonId;
    protected long TestScoringId => dbContext.Scorings.First().ScoringId;
    protected long TestScheduleId => dbContext.Schedules.First().ScheduleId;
    protected long TestResultId => dbContext.ScoredEventResults.First().ResultId;
    protected long TestPointRuleId => dbContext.PointRules.First().PointRuleId;
    protected long TestResultConfigId => dbContext.ResultConfigurations.First().ResultConfigId;
    protected long TestReviewId => dbContext.IncidentReviews.First().ReviewId;
    protected long TestMemberId => dbContext.Members.First().Id;
    protected long TestCommentId => dbContext.ReviewComments.First().CommentId;
    protected long testVoteCategory = 1;
    protected const string testUserName = "TestUser";
    protected const string testUserId = "a0031cbe-a28b-48ac-a6db-cdca446a8162";
    protected static IEnumerable<string> testLeagueRoles = new string[] { LeagueRoles.Member };

    public HandlersTestsBase()
    {
        logger = Mock.Of<ILogger<THandler>>();
    }

    protected abstract TRequest DefaultRequest();

    protected abstract THandler CreateTestHandler(LeagueDbContext dbContext, IValidator<TRequest> validator);

    protected ValidationResult ValidationFailed()
    {
        return new ValidationResult(new ValidationFailure[] { new ValidationFailure("Property", "Error") });
    }

    protected virtual void DefaultPreTestAssertions(TRequest request, LeagueDbContext dbContext)
    {
    }

    protected virtual void DefaultAssertions(TRequest request, TResult result, LeagueDbContext dbContext)
    {
        result.Should().NotBeNull();
    }

    protected virtual void AssertVersion(IVersionEntity expected, IVersionModel result)
    {
        result.CreatedByUserId.Should().Be(expected.CreatedByUserId);
        result.CreatedByUserName.Should().Be(expected.CreatedByUserName);
        result.CreatedOn.Should().Be(expected.CreatedOn);
        result.LastModifiedByUserId.Should().Be(expected.LastModifiedByUserId);
        result.LastModifiedByUserName.Should().Be(expected.LastModifiedByUserName);
        result.LastModifiedOn.Should().Be(expected.LastModifiedOn);
    }

    protected virtual void AssertCreated(LeagueUser user, DateTime time, IVersionModel result)
    {
        result.CreatedByUserId.Should().Be(user.Id);
        result.CreatedByUserName.Should().Be(user.Name);
        result.CreatedOn.Should().BeCloseTo(time, TimeSpan.FromSeconds(10));
        result.LastModifiedByUserId.Should().Be(user.Id);
        result.LastModifiedByUserName.Should().Be(user.Name);
        result.LastModifiedOn.Should().BeCloseTo(time, TimeSpan.FromSeconds(10));
    }

    protected virtual void AssertChanged(LeagueUser user, DateTime time, IVersionModel result)
    {
        result.LastModifiedByUserId.Should().Be(user.Id);
        result.LastModifiedByUserName.Should().Be(user.Name);
        result.LastModifiedOn.Should().BeCloseTo(time, TimeSpan.FromSeconds(10));
    }

    protected virtual ClaimsPrincipal DefaultPrincipal(string leagueName = testLeagueName, string userName = testUserName,
        string userId = testUserId, IEnumerable<string>? roles = default)
    {
        roles ??= testLeagueRoles;
        var builder = StaticIdentityBuilders.BuildPrincipal()
            .WithName(userName)
            .WithIdentifier(userId);
        foreach (var role in roles)
        {
            builder.WithRole(LeagueRoles.GetLeagueRoleName(leagueName, role));
        }
        return builder.Create();
    }

    protected virtual LeagueUser DefaultUser(string leagueName = testLeagueName, string userName = testUserName,
        string userId = testUserId, IEnumerable<string>? roles = default)
    {
        return new LeagueUser(leagueName, DefaultPrincipal(leagueName, userName, userId, roles));
    }

    /// <summary>
    /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and perform default assertions
    /// <para/>Assertions can be modified by overriding <see cref="DefaultAssertions"/>
    /// </summary>
    /// <returns><typeparamref name="TResult"/> Result of the handle method</returns>
    public virtual async Task<TResult> ShouldHandleDefault()
    {
        using var dbContext = accessMockHelper.CreateMockDbContext();

        var request = DefaultRequest();
        var handler = CreateTestHandler(dbContext, MockHelpers.TestValidator<TRequest>());

        DefaultPreTestAssertions(request, dbContext);
        var result = await handler.Handle(request, default);
        DefaultAssertions(request, result, dbContext);

        return result;
    }

    /// <summary>
    /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and assert throwing <see cref="ResourceNotFoundException"/>
    /// </summary>
    /// <param name="request">Request for a not existing resource</param>
    public async Task HandleNotFoundRequestAsync(TRequest? request = null)
    {
        request ??= DefaultRequest();
        var act = async () => await HandleSpecialAsync(request, null);
        await act.Should()
            .ThrowAsync<ResourceNotFoundException>();
    }

    /// <summary>
    /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and perform the provided assertions
    /// </summary>
    /// <param name="request">Request for a not existing resource</param>
    /// <param name="assertions">Assertions to be performed</param>
    /// <returns></returns>
    public virtual async Task<TResult> HandleSpecialAsync(TRequest request, Action<TRequest, TResult, LeagueDbContext>? assertions,
        Action<TRequest, LeagueDbContext>? preTestAssertions = default)
    {
        using var dbContext = accessMockHelper.CreateMockDbContext();

        var handler = CreateTestHandler(dbContext, MockHelpers.TestValidator<TRequest>());

        preTestAssertions?.Invoke(request, dbContext);
        var result = await handler.Handle(request, default);
        assertions?.Invoke(request, result, dbContext);

        return result;
    }

    /// <summary>
    /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and assert throwing <see cref="ValidationException"/>
    /// </summary
    public virtual async Task ShouldHandleValidationFailed()
    {
        using var dbContext = accessMockHelper.CreateMockDbContext();
        var mockValidator = MockHelpers.MockValidator<TRequest>();
        mockValidator.Setup(x => x.Validate(It.IsAny<TRequest>()))
            .Returns(ValidationFailed());
        mockValidator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), default))
            .ReturnsAsync(ValidationFailed());

        var request = DefaultRequest();
        var handler = CreateTestHandler(dbContext, mockValidator.Object);

        var act = async () => await handler.Handle(request, default);
        await act.Should()
            .ThrowAsync<ValidationException>();
    }
}
