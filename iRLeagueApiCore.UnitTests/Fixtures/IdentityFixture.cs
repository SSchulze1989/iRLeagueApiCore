using FluentValidation;
using FluentValidation.Results;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Handlers.Admin;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Fixtures
{
    public class IdentityFixture
    {
        public readonly string testLeague = "TestLeague";
        public readonly ApplicationUser validUser = new ApplicationUser()
        {
            UserName = "TestUser"
        };
        public readonly string testRole = LeagueRoles.Organizer;
        public string testLeagueRoleName => LeagueRoles.GetLeagueRoleName(testLeague, testRole);
        public IdentityRole testLeagueRole => new IdentityRole(testLeagueRoleName);
        public readonly string[] inRoles = new string[]
        {
            LeagueRoles.Member,
            LeagueRoles.Steward
        };

        [Fact]
        public async Task TestDefaultSetup()
        {
            var testUserManager = GetDefaultMockUserManager().Object;
            var testRoleManager = GetDefaultMockRoleManager().Object;
            var testValidator = GetDefaultValidator<Unit>().Object;
            Assert.NotNull(await testUserManager.FindByNameAsync(validUser.UserName));
            Assert.True(await testRoleManager.RoleExistsAsync(testLeagueRoleName));
            Assert.True(testValidator.Validate(Unit.Value).IsValid);
        }

        public Mock<UserManager<ApplicationUser>> GetDefaultMockUserManager()
        {
            var mockUserManager = MockHelpers.MockUserManager<ApplicationUser>();
            mockUserManager.Setup(x => x.FindByNameAsync(It.Is<string>(x => x == validUser.UserName)))
                .ReturnsAsync(validUser);
            return mockUserManager;
        }

        public Mock<RoleManager<IdentityRole>> GetDefaultMockRoleManager()
        {
            var mockRoleManager = MockHelpers.MockRoleManager<IdentityRole>();
            mockRoleManager.Setup(x => x.RoleExistsAsync(It.Is<string>(x => x == testLeagueRoleName)))
                .ReturnsAsync(true);
            return mockRoleManager;
        }

        public Mock<IValidator<T>> GetDefaultValidator<T>()
        {
            var mockValidator = MockHelpers.MockValidator<T>();
            mockValidator.SetReturnsDefault(new ValidationResult());
            mockValidator.SetReturnsDefault(Task.FromResult(new ValidationResult()));
            return mockValidator;
        }

        public ValidationResult GetSuccessResult()
        {
            return new ValidationResult();
        }

        public ValidationResult GetFailedResult()
        {
            return new ValidationResult(new ValidationFailure[]
            {
                new ValidationFailure("Property1", "Message1"),
                new ValidationFailure("Property2", "Message2")
            });
        }
    }

    public static class IndentiyFixtureExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T single)
        {
            return new T[] { single };
        }
    }
}
