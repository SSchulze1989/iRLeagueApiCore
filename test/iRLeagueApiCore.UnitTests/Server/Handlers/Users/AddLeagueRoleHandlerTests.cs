using iRLeagueApiCore.Common;
using iRLeagueApiCore.Server.Handlers.Users;
using iRLeagueApiCore.UnitTests.Fixtures;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Users;
public sealed class AddLeagueRoleHandlerTests : UserHandlerTestsBase<AddLeagueRoleHandler, AddLeagueRoleRequest>
{
    public AddLeagueRoleHandlerTests(IdentityFixture identityFixture) : 
        base(identityFixture)
    {
    }

    [Fact]
    public async Task ShouldAddUserToRole()
    {
        var testUser = CreateTestUser();
        var testRole = CreateTestRole(identityFixture.testLeague);
        var request = CreateRequest(identityFixture.testLeague, testUser.Id, LeagueRoles.GetRoleName(testRole.Name)!);
        var sut = CreateSut();

        await sut.Handle(request, default);

        identityFixture.UserRoles.Should().Contain(x => x.Value.Any(role => role.Id == testRole.Id));
    }

    [Fact]
    public async Task ShoulCreateNonExistingRole()
    {
        var testUser = CreateTestUser();
        var testRoleName = LeagueRoles.Steward;
        var testLeagueRoleName = LeagueRoles.GetLeagueRoleName(identityFixture.testLeague, testRoleName);
        var request = CreateRequest(identityFixture.testLeague, testUser.Id, testRoleName);
        var sut = CreateSut();

        identityFixture.Roles.Should().NotContain(x => x.Value.Name == testLeagueRoleName);
        await sut.Handle(request, default);

        identityFixture.Roles.Should().Contain(x => x.Value.Name == testLeagueRoleName);
    }

    private AddLeagueRoleRequest CreateRequest(string leagueName, string userId, string roleName)
    {
        return new AddLeagueRoleRequest(leagueName, userId, roleName);
    }

    private AddLeagueRoleHandler CreateSut()
    {
        return new AddLeagueRoleHandler(logger, identityFixture.UserManager, identityFixture.RoleManager, validators);
    }
}
