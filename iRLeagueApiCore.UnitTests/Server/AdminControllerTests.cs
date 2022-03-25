using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        IUserStore<ApplicationUser> UserStore { get; }
        IRoleStore<IdentityRole> RoleStore { get; }
        ApplicationUser MockUser { get; }

        const string TestUserName = "TestRoleUser";
        const string TestRoleName = LeagueRoles.Member;
        const string TestLeagueName = "TestLeague";
        string TestLeagueRoleName = LeagueRoles.GetLeagueRoleName(TestLeagueName, TestRoleName);

        public AdminControllerTests(DbTestFixture fixture)
        {
            Fixture = fixture;
            UserStore = new Mock<IUserStore<ApplicationUser>>().Object;
            RoleStore = new Mock<IRoleStore<IdentityRole>>().Object;

            var userMock = new Mock<ApplicationUser>();
            userMock.SetupAllProperties();
            MockUser = userMock.Object;
            MockUser.UserName = TestUserName;
        }

        [Fact]
        public async Task GiveRoleValid()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleAdded = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.AddToRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleAdded = true; return IdentityResult.Success; });

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await userManagerMock.Object.AddToRoleAsync(MockUser, TestLeagueRoleName)).Succeeded);
            Debug.Assert(await roleManagerMock.Object.RoleExistsAsync(TestLeagueRoleName));

            roleAdded = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            Assert.IsType<OkObjectResult>(result);
            Assert.True(roleAdded);
        }

        [Fact]
        public async Task GiveRoleUnknownUser()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleAdded = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(default(ApplicationUser));
            userManagerMock.Setup(x => x.AddToRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleAdded = true; return IdentityResult.Failed(); });

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == null);

            roleAdded = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResultResponse>(badRequest.Value);
            Assert.Equal("User not found", response.Result);
            Assert.False(roleAdded);
        }

        [Fact]
        public async Task GiveRoleCreated()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleAdded = false;
            bool roleCreated = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.AddToRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleAdded = true; return IdentityResult.Success; });

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(false);
            roleManagerMock.Setup(x => x.CreateAsync(It.Is<IdentityRole>(x => x.Name == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleCreated = true; return IdentityResult.Success; });

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await roleManagerMock.Object.CreateAsync(new IdentityRole(TestLeagueRoleName))).Succeeded);

            roleCreated = false;
            roleAdded = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            Assert.IsType<OkObjectResult>(result);
            Assert.True(roleCreated);
            Assert.True(roleAdded);
        }

        [Fact]
        public async Task GiveRoleInvalidRole()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleAdded = false;
            bool roleCreated = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.AddToRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleAdded = true; return IdentityResult.Success; });

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(false);
            roleManagerMock.Setup(x => x.CreateAsync(It.Is<IdentityRole>(x => x.Name == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleCreated = true; return IdentityResult.Success; });

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await roleManagerMock.Object.RoleExistsAsync(TestLeagueRoleName)) == false);
            Debug.Assert((await roleManagerMock.Object.CreateAsync(new IdentityRole(TestLeagueRoleName))).Succeeded);

            roleCreated = false;
            roleAdded = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = "InvalidRole" });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResultResponse>(badRequest.Value);
            Assert.Equal("Invalid role", response.Result);
            Assert.False(roleCreated);
            Assert.False(roleAdded);
        }

        [Fact]
        public async Task GiveRoleCreationFailed()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleAdded = false;
            bool roleCreated = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(false);
            roleManagerMock.Setup(x => x.CreateAsync(It.Is<IdentityRole>(x => x.Name == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleCreated = false; return IdentityResult.Failed(); });

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await roleManagerMock.Object.CreateAsync(new IdentityRole(TestLeagueRoleName))).Succeeded == false);

            roleCreated = false;
            roleAdded = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.False(roleCreated);
            Assert.False(roleAdded);
        }

        [Fact]
        public async Task GiveRoleAddRoleFailed()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleAdded = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.AddToRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleAdded = false; return IdentityResult.Failed(); });

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await roleManagerMock.Object.RoleExistsAsync(TestLeagueRoleName)));
            Debug.Assert((await userManagerMock.Object.AddToRoleAsync(MockUser, TestLeagueRoleName)).Succeeded == false);

            roleAdded = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.False(roleAdded);
        }

        [Fact]
        public async Task RevokeRoleValid()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleRevoked = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.RemoveFromRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleRevoked = true; return IdentityResult.Success; });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await userManagerMock.Object.RemoveFromRoleAsync(MockUser, TestLeagueRoleName)).Succeeded);
            Debug.Assert(await userManagerMock.Object.IsInRoleAsync(MockUser, TestLeagueRoleName));
            Debug.Assert(await roleManagerMock.Object.RoleExistsAsync(TestLeagueRoleName));

            roleRevoked = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.RevokeRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            Assert.IsType<OkObjectResult>(result);
            Assert.True(roleRevoked);
        }

        [Fact]
        public async Task RevokeRoleUnknownUser()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(default(ApplicationUser));

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == null);
            Debug.Assert(await roleManagerMock.Object.RoleExistsAsync(TestLeagueRoleName));

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.RevokeRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResultResponse>(badRequest.Value);
            Assert.Equal("User not found", response.Result);
        }

        [Fact]
        public async Task RevokeRoleInvalidRole()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleCreated = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(false);
            roleManagerMock.Setup(x => x.CreateAsync(It.Is<IdentityRole>(x => x.Name == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleCreated = true; return IdentityResult.Success; });

            Debug.Assert((await userManagerMock.Object.FindByNameAsync(TestUserName)) == MockUser);
            Debug.Assert((await roleManagerMock.Object.RoleExistsAsync(TestLeagueRoleName)) == false);
            Debug.Assert((await roleManagerMock.Object.CreateAsync(new IdentityRole(TestLeagueRoleName))).Succeeded);

            roleCreated = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = "InvalidRole" });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResultResponse>(badRequest.Value);
            Assert.Equal("Invalid role", response.Result);
            Assert.False(roleCreated);
        }

        [Fact]
        public async Task RevokeRoleNotInRole()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleRevoked = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.RemoveFromRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => { roleRevoked = true; return IdentityResult.Success; });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(false);

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert(await userManagerMock.Object.IsInRoleAsync(MockUser, TestLeagueRoleName) == false);

            roleRevoked = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.RevokeRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var respones = Assert.IsType<ResultResponse>(badRequest.Value);
            Assert.Equal("Not in role", respones.Result);
            Assert.False(roleRevoked);
        }

        [Fact]
        public async Task RevokeRoleFailed()
        {
            var userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            var roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            bool roleRevoked = false;

            userManagerMock.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == TestUserName)))
                .ReturnsAsync(MockUser);
            userManagerMock.Setup(x => x.RemoveFromRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(() => IdentityResult.Failed());
            userManagerMock.Setup(x => x.IsInRoleAsync(It.Is<ApplicationUser>(x => x == MockUser), It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == TestLeagueRoleName)))
                .ReturnsAsync(true);

            Debug.Assert((await userManagerMock.Object.RemoveFromRoleAsync(MockUser, TestLeagueRoleName)).Succeeded == false);

            roleRevoked = false;

            var controller = Fixture.AddLeagueAdminControllerContext(
                new AdminController(_mockLogger, userManagerMock.Object, roleManagerMock.Object), TestLeagueName);
            var result = await controller.RevokeRole(TestLeagueName, new UserRoleModel() { UserName = TestUserName, RoleName = TestRoleName });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.False(roleRevoked);
        }
    }
}
