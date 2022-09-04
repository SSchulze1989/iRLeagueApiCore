using FluentAssertions;
using FluentIdentityBuilder;
using FluentValidation;
using FluentValidation.Results;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers
{
    public abstract class HandlersTestsBase<THandler, TRequest, TResult> : IClassFixture<DbTestFixture> 
        where THandler : IRequestHandler<TRequest, TResult> 
        where TRequest : class, IRequest<TResult>
    {
        protected readonly DbTestFixture fixture;
        protected readonly ILogger<THandler> logger;

        protected const long testLeagueId = 1;
        protected const string testLeagueName = "TestLeague";
        protected const long testSeasonId = 1;
        protected const long testScoringId = 1;
        protected const long testScheduleId = 1;
        protected const long testResultId = 1;
        protected const long testPointRuleId = 1;
        protected const long testResultConfigId = 1;
        protected const string testUserName = "TestUser";
        protected const string testUserId = "a0031cbe-a28b-48ac-a6db-cdca446a8162";
        protected static IEnumerable<string> testLeagueRoles = new string[] { LeagueRoles.Member };

        public HandlersTestsBase(DbTestFixture fixture)
        {
            this.fixture = fixture;
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

        protected virtual ClaimsPrincipal DefaultPrincipal(string leagueName = testLeagueName, string userName = testUserName,
            string userId = testUserId, IEnumerable<string> roles = default)
        {
            if (roles == null)
            {
                roles = testLeagueRoles;
            }
            var builder = StaticIdentityBuilders.BuildPrincipal()
                .WithName(userName)
                .WithIdentifier(userId);
            foreach(var role in roles)
            {
                builder.WithRole(LeagueRoles.GetLeagueRoleName(leagueName, role));
            }
            return builder.Create();
        }

        protected virtual LeagueUser DefaultUser(string leagueName = testLeagueName, string userName = testUserName, 
            string userId = testUserId, IEnumerable<string> roles = default)
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
            using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var dbContext = fixture.CreateDbContext();

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
        public async Task HandleNotFoundRequestAsync(TRequest request = null)
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
        public virtual async Task<TResult> HandleSpecialAsync(TRequest request, Action<TRequest, TResult, LeagueDbContext> assertions, 
            Action<TRequest, LeagueDbContext> preTestAssertions = default)
        {
            using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var dbContext = fixture.CreateDbContext();

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
            using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var dbContext = fixture.CreateDbContext();
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
}
