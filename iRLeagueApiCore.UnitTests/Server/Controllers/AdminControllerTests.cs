using FluentValidation;
using FluentValidation.Results;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Handlers.Admin;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    public class AdminControllerTests
    {
        readonly ILogger<AdminController> _mockLogger = new Mock<ILogger<AdminController>>().Object;
        ApplicationUser MockUser { get; }

        const string TestUserName = "TestRoleUser";
        const string TestRoleName = LeagueRoles.Member;
        const string TestLeagueName = "TestLeague";
        string TestLeagueRoleName = LeagueRoles.GetLeagueRoleName(TestLeagueName, TestRoleName);

        public AdminControllerTests()
        {
            var userMock = new Mock<ApplicationUser>();
            userMock.SetupAllProperties();
            MockUser = userMock.Object;
            MockUser.UserName = TestUserName;
        }

        private static ValidationException ValdiationFailed()
        {
            return new ValidationException("Test validation failed");
        }

        [Fact]
        public async void ListUsersRequestValid()
        {
            var userList = new List<GetAdminUserModel>()
            {
                new GetAdminUserModel() { UserName = TestUserName, Roles = new string[] { "Role1", "Role2" } }
            };
            var mediator = MockHelpers.TestMediator<ListUsersRequest, IEnumerable<GetAdminUserModel>>(result: userList);
            var controller = AddContexts.AddAdminControllerContext(new AdminController(_mockLogger, mediator));
            var result = await controller.ListUsers(TestLeagueName);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var usersResult = Assert.IsAssignableFrom<IEnumerable<GetAdminUserModel>>(okResult.Value);
            Assert.Equal(TestUserName, usersResult.First().UserName);
        }

        [Fact]
        public async void ListUsersValidationFailed()
        {
            var mediator = MockHelpers.TestMediator<ListUsersRequest, IEnumerable<GetAdminUserModel>>(throws: ValdiationFailed());
            var controller = AddContexts.AddAdminControllerContext(new AdminController(_mockLogger, mediator));
            var result = await controller.ListUsers(TestLeagueName);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async void GiveRoleRequestValid()
        {
            const string roleName = "TestRole";
            var mediator = MockHelpers.TestMediator<GiveRoleRequest, Unit>();
            var controller = AddContexts.AddAdminControllerContext(new AdminController(_mockLogger, mediator));
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel { UserName = TestUserName, RoleName = roleName });

            Assert.IsType<OkObjectResult>(result);
            var mediatorMock = Mock.Get(mediator);
            mediatorMock.Verify(x => x.Send(It.IsAny<GiveRoleRequest>(), default));
        }

        [Fact]
        public async void GiveRoleValidationFailed()
        {
            const string roleName = "TestRole";
            var mediator = MockHelpers.TestMediator<GiveRoleRequest, Unit>(throws: ValdiationFailed());
            var controller = AddContexts.AddAdminControllerContext(new AdminController(_mockLogger, mediator));
            var result = await controller.GiveRole(TestLeagueName, new UserRoleModel { UserName = TestUserName, RoleName = roleName });

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
