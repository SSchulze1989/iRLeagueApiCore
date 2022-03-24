using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server
{
    public class AdminControllerTests : IClassFixture<DbTestFixture>
    {
        DbTestFixture Fixture { get; }
        readonly ILogger<AdminController> _mockLogger = new Mock<ILogger<AdminController>>().Object;

        public AdminControllerTests(DbTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async void TestGiveRole()
        {
            const string TestUserName = "TestRoleUser";
            const string TestRoleName = "Member";
            const string TestLeagueName = "TestLeague";
            const string TestLeagueRoleName = "TestLeague:Member";

            var userManagerMock = new Mock<UserManager<ApplicationUser>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>();
            var userMock = new Mock<ApplicationUser>();

            userMock.SetupAllProperties();
            var user = userMock.Object;
            user.UserName = TestUserName;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.AddToRoleAsync(It.Is<ApplicationUser>(x => x == user), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(IdentityResult.Success);

            var controller = Fixture.AddAdminControllerContext(new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object));

        }
    }
}
