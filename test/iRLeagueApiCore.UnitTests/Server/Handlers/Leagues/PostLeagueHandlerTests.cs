﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues;

[Collection("DbTestFixture")]
public sealed class PostLeagueDbTestFixture : HandlersTestsBase<PostLeagueHandler, PostLeagueRequest, LeagueModel>
{
    private const string postLeagueName = "PostLeague";

    public PostLeagueDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override PostLeagueHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostLeagueRequest> validator)
    {
        return new PostLeagueHandler(logger, dbContext, new IValidator<PostLeagueRequest>[] { validator });
    }

    protected override PostLeagueRequest DefaultRequest()
    {
        var model = new PostLeagueModel()
        {
            Name = postLeagueName,
            NameFull = "Full test league name"
        };
        return CreateRequest(DefaultUser(), model);
    }

    protected override void DefaultAssertions(PostLeagueRequest request, LeagueModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.Id.Should().NotBe(0);
        result.Name.Should().Be(expected.Name);
        result.NameFull.Should().Be(expected.NameFull);
        AssertCreated(request.User, DateTime.UtcNow, result);
        base.DefaultAssertions(request, result, dbContext);
    }

    private PostLeagueRequest CreateRequest(LeagueUser user, PostLeagueModel model)
    {
        return new PostLeagueRequest(user, model);
    }

    [Fact]
    public override async Task<LeagueModel> ShouldHandleDefault()
    {
        var result = await base.ShouldHandleDefault();
        return result;
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
